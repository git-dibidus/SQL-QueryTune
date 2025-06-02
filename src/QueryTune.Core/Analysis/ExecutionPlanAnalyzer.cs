using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using QueryTune.Core.Models;

namespace QueryTune.Core.Analysis
{
    public class ExecutionPlanAnalyzer
    {
        public static List<OptimizationSuggestion> AnalyzePlan(DataTable planTable)
        {
            var suggestions = new List<OptimizationSuggestion>();

            if (planTable.Rows.Count == 0) return suggestions;

            var xmlPlan = XElement.Parse(planTable.Rows[0][0].ToString());

            // Check for table scans (usually bad)
            var scans = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value == "Table Scan");

            foreach (var scan in scans)
            {
                var table = scan.Attribute("Table")?.Value;
                suggestions.Add(new OptimizationSuggestion
                {
                    Type = SuggestionType.CreateIndex,
                    Description = $"Table scan detected on {table}. Consider adding an appropriate index.",
                    ObjectName = table,
                    Impact = SuggestionImpact.High
                });
            }

            // Check for key lookups (RID or Key)
            var lookups = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value.Contains("Lookup") == true);

            foreach (var lookup in lookups)
            {
                var objectName = lookup.Attribute("Object")?.Value;
                suggestions.Add(new OptimizationSuggestion
                {
                    Type = SuggestionType.IncludeColumns,
                    Description = $"Key lookup detected on {objectName}. Consider adding included columns to the index.",
                    ObjectName = objectName,
                    Impact = SuggestionImpact.Medium
                });
            }

            // Check for expensive sorts
            var sorts = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value == "Sort")
                .Select(e => new {
                    Element = e,
                    Cost = decimal.Parse(e.Attribute("EstimatedTotalSubtreeCost")?.Value ?? "0")
                })
                .Where(x => x.Cost > 0.1m); // Threshold for "expensive" sort

            foreach (var sort in sorts)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Type = SuggestionType.QueryRewrite,
                    Description = $"Expensive sort operation detected (cost: {sort.Cost}). Consider adding ORDER BY columns to an index.",
                    Impact = SuggestionImpact.Medium
                });
            }

            // Check for implicit conversions
            var conversions = xmlPlan.Descendants()
                .Where(e => e.Attribute("Implicit")?.Value == "1");

            foreach (var conv in conversions)
            {
                var fromType = conv.Attribute("ConvertFrom")?.Value;
                var toType = conv.Attribute("ConvertTo")?.Value;
                suggestions.Add(new OptimizationSuggestion
                {
                    Type = SuggestionType.QueryRewrite,
                    Description = $"Implicit conversion from {fromType} to {toType} detected. This can impact performance.",
                    Impact = SuggestionImpact.Low
                });
            }

            return suggestions;
        }
    }
}
