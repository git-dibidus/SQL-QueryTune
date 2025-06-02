using Microsoft.Data.SqlClient;
using QueryTune.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace QueryTune.Core.Analysis
{
    public class StatisticsAnalyzer
    {
        private readonly string _connectionString;

        public StatisticsAnalyzer(string connectionString) => _connectionString = connectionString;

        public List<OptimizationSuggestion> CheckStatistics(DataTable planTable)
        {
            var suggestions = new List<OptimizationSuggestion>();
            if (planTable.Rows.Count == 0) return suggestions;

            var xmlPlan = XElement.Parse(planTable.Rows[0][0].ToString());

            // Find all tables referenced in the plan
            var tables = xmlPlan.Descendants()
                .Select(e => e.Attribute("Table")?.Value ?? e.Attribute("Object")?.Value)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            foreach (var table in tables)
            {
                // Check when stats were last updated
                var statsQuery = $@"
                SELECT 
                    s.name AS stats_name,
                    STATS_DATE(s.object_id, s.stats_id) AS last_updated,
                    DATEDIFF(DAY, STATS_DATE(s.object_id, s.stats_id), GETDATE()) AS days_since_update
                FROM sys.stats s
                JOIN sys.objects o ON s.object_id = o.object_id
                WHERE o.name = @TableName
                ORDER BY last_updated ASC";

                using var cmd = new SqlCommand(statsQuery, connection);
                cmd.Parameters.AddWithValue("@TableName", table);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var statsName = reader["stats_name"].ToString();
                    var daysSinceUpdate = Convert.ToInt32(reader["days_since_update"]);

                    if (daysSinceUpdate > 7) // Threshold for "stale" statistics
                    {
                        suggestions.Add(new OptimizationSuggestion
                        {
                            Type = SuggestionType.UpdateStatistics,
                            Description = $"Statistics on {table}.{statsName} were last updated {daysSinceUpdate} days ago.",
                            ObjectName = $"{table}.{statsName}",
                            RecommendedAction = $"UPDATE STATISTICS [{table}] [{statsName}]",
                            Impact = daysSinceUpdate > 30 ? SuggestionImpact.High : SuggestionImpact.Medium
                        });
                    }
                }
                reader.Close();
            }

            return suggestions;
        }
    }
}
