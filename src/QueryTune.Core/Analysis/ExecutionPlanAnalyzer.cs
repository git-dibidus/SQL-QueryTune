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
            var ns = xmlPlan.Name.Namespace; // Get the XML namespace

            // Check for table scans (usually bad)
            var scans = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value == "Table Scan")
                .Select(e => new
                {
                    Element = e,
                    Object = e.Descendants(ns + "Object")
                        .FirstOrDefault()
                        ?.Attribute("Table")
                        ?.Value
                });

            foreach (var scan in scans)
            {
                if (!string.IsNullOrEmpty(scan.Object))
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Type = SuggestionType.CreateIndex,
                        Description = $"Table scan detected on {scan.Object}. Consider adding an appropriate index.",
                        ObjectName = scan.Object,
                        Impact = SuggestionImpact.High
                    });
                }
            }

            // Check for key lookups (RID or Key)
            var lookups = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value?.Contains("Lookup") == true)
                .Select(e => new
                {
                    Element = e,
                    Object = e.Descendants(ns + "Object")
                        .FirstOrDefault()
                        ?.Attribute("Table")
                        ?.Value
                });

            foreach (var lookup in lookups)
            {
                if (!string.IsNullOrEmpty(lookup.Object))
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Type = SuggestionType.IncludeColumns,
                        Description = $"Key lookup detected on {lookup.Object}. Consider adding included columns to the index.",
                        ObjectName = lookup.Object,
                        Impact = SuggestionImpact.Medium
                    });
                }
            }

            // Check for expensive sorts
            var sorts = xmlPlan.Descendants()
                .Where(e => e.Attribute("PhysicalOp")?.Value == "Sort")
                .Select(e => new
                {
                    Element = e,
                    Cost = decimal.Parse(e.Attribute("EstimatedTotalSubtreeCost")?.Value ?? "0"),
                    Object = e.Descendants(ns + "Object")
                        .FirstOrDefault()
                        ?.Attribute("Table")
                        ?.Value
                })
                .Where(x => x.Cost > 0.1m); // Threshold for "expensive" sort

            foreach (var sort in sorts)
            {
                suggestions.Add(new OptimizationSuggestion
                {
                    Type = SuggestionType.QueryRewrite,
                    Description = $"Expensive sort operation detected (cost: {sort.Cost}){(sort.Object != null ? $" on table {sort.Object}" : "")}. Consider adding ORDER BY columns to an index.",
                    ObjectName = sort.Object,
                    Impact = SuggestionImpact.Medium
                });
            }

            // Check for implicit conversions
            var conversions = xmlPlan.Descendants()
                .Where(e => e.Elements()
                    .Any(c => c.Attribute("FunctionName")?.Value == "CONVERT_IMPLICIT"));

            foreach (var conv in conversions)
            {
                var convertNode = conv.Elements()
                    .First(c => c.Attribute("FunctionName")?.Value == "CONVERT_IMPLICIT");
                
                var fromType = convertNode.Elements()
                    .FirstOrDefault()
                    ?.Attribute("DataType")
                    ?.Value;
                
                var toType = convertNode
                    .Attribute("DataType")
                    ?.Value;

                if (!string.IsNullOrEmpty(fromType) && !string.IsNullOrEmpty(toType))
                {
                    var objectName = conv.Descendants(ns + "ColumnReference")
                        .FirstOrDefault()
                        ?.Attribute("Table")
                        ?.Value;

                    suggestions.Add(new OptimizationSuggestion
                    {
                        Type = SuggestionType.QueryRewrite,
                        Description = $"Implicit conversion from {fromType} to {toType} detected{(objectName != null ? $" on table {objectName}" : "")}. This can impact performance.",
                        ObjectName = objectName,
                        Impact = SuggestionImpact.Low
                    });
                }
            }

            return suggestions;
        }
    }
}
