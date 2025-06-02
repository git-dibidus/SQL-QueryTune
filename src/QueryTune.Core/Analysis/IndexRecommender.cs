using QueryTune.Core.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace QueryTune.Core.Analysis
{
    public class IndexRecommender
    {
        public static List<OptimizationSuggestion> RecommendIndexes(DataTable planTable, DataTable metricsTable)
        {
            var suggestions = new List<OptimizationSuggestion>();
            if (planTable.Rows.Count == 0) return suggestions;

            var xmlPlan = XElement.Parse(planTable.Rows[0][0].ToString());

            // Find all scan operations and their predicates
            var scanOps = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value.Contains("Scan") == true)
                .Select(e => new {
                    Element = e,
                    Table = e.Attribute("Table")?.Value ?? e.Attribute("Object")?.Value,
                    Predicate = e.Descendants().FirstOrDefault(d => d.Name.LocalName == "Predicate")?.Value
                });

            foreach (var scan in scanOps)
            {
                if (string.IsNullOrEmpty(scan.Predicate)) continue;

                // Extract columns used in predicates
                var columns = ExtractColumnsFromPredicate(scan.Predicate);

                if (columns.Count != 0)
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Type = SuggestionType.CreateIndex,
                        Description = $"Consider creating an index on {scan.Table} for columns used in WHERE/JOIN: {string.Join(", ", columns)}",
                        ObjectName = scan.Table,
                        RecommendedAction = GenerateCreateIndexStatement(scan.Table, columns),
                        Impact = SuggestionImpact.High
                    });
                }
            }

            return suggestions;
        }

        private static List<string> ExtractColumnsFromPredicate(string predicate)
        {
            // Simplified parsing - in reality you'd need a more robust approach
            var columns = new List<string>();

            // Look for patterns like [ColumnName] = or [ColumnName] >
            var matches = System.Text.RegularExpressions.Regex.Matches(
                predicate,
                @"\[([^\]]+)\]");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Success && !columns.Contains(match.Groups[1].Value))
                {
                    columns.Add(match.Groups[1].Value);
                }
            }

            return columns;
        }

        private static string GenerateCreateIndexStatement(string table, List<string> columns)
        {
            if (columns.Count == 0) return string.Empty;

            var indexName = $"IX_{table}_{string.Join("_", columns.Take(3))}";
            return $"CREATE NONCLUSTERED INDEX [{indexName}] ON [{table}] ([{string.Join("], [", columns)}])";
        }
    }
}
