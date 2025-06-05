using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QueryTune.Core.Models;

namespace QueryTune.Core.Reporting
{
    public class HtmlReportGenerator
    {
        private static void AddMetricRow(StringBuilder sb, string metricName, string value)
        {
            sb.AppendLine($"<tr><td><strong>{metricName}</strong></td><td>{value}</td></tr>");
        }
        
        private static void AddMetricIfExists(StringBuilder sb, DataRow row, string columnName, string displayName, string format = null)
        {
            // Check if the column exists in the DataRow
            if (row.Table.Columns.Contains(columnName))
            {
                AddMetricRow(sb, displayName, FormatValue(row[columnName], format));
            }
        }
        
        private static bool IsPerformanceMetric(string columnName)
        {
            // These keywords usually indicate performance-related columns
            string[] performanceKeywords = new[] {
                "time", "duration", "cpu", "reads", "writes", "rows", "memory",
                "execution", "worker", "elapsed", "io", "count", "wait", "cost"
            };
            
            return performanceKeywords.Any(keyword => columnName.ToLower().Contains(keyword));
        }
        
        private static string FormatColumnName(string columnName)
        {
            return string.Join(" ", columnName.Split('_')
                .Select(word => word.Length > 0 ? 
                    char.ToUpper(word[0]) + (word.Length > 1 ? word.Substring(1).ToLower() : "") : 
                    ""));
        }

        private static string FormatValue(object value, string format = null)
        {
            if (value == null || value == DBNull.Value)
            {
                return "N/A";
            }
            
            if (value is int || value is long || value is double || value is decimal)
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return string.Format($"{{0:{format}}}", value);
                }
            }
            
            return value.ToString();
        }
        
        private static string GetHtmlHeader()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>SQL Query Analysis</title>");
            sb.AppendLine("<style>");            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("h1 { color: #2a5885; }");
            sb.AppendLine(".query { background: #f5f5f5; padding: 10px; border-radius: 5px; }");
            sb.AppendLine(".metrics { margin: 20px 0; }");
            sb.AppendLine(".metrics table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine(".metrics th, .metrics td { padding: 8px; text-align: left; border: 1px solid #ddd; }");
            sb.AppendLine(".metrics th { background-color: #f2f2f2; }");
            sb.AppendLine(".metrics tr:nth-child(even) { background-color: #f9f9f9; }");
            sb.AppendLine(".metrics tr:hover { background-color: #f1f1f1; }");
            sb.AppendLine(".suggestions { margin-top: 30px; }");
            sb.AppendLine(".suggestion { margin-bottom: 15px; padding: 10px; border-left: 4px solid; }");
            sb.AppendLine(".high-impact { border-color: #e74c3c; background: #fde8e6; }");
            sb.AppendLine(".medium-impact { border-color: #f39c12; background: #fef5e6; }");
            sb.AppendLine(".low-impact { border-color: #3498db; background: #eaf2f8; }");
            sb.AppendLine(".impact-label { display: inline-block; padding: 2px 5px; border-radius: 3px; color: white; font-size: 0.8em; }");
            sb.AppendLine(".high-label { background: #e74c3c; }");
            sb.AppendLine(".medium-label { background: #f39c12; }");
            sb.AppendLine(".low-label { background: #3498db; }");
            sb.AppendLine(".error { border-left: 4px solid #e74c3c; background: #fde8e6; padding: 15px; margin: 20px 0; }");
            sb.AppendLine(".error h2 { color: #e74c3c; margin-top: 0; }");
            sb.AppendLine(".error pre { background: #fff; padding: 10px; border-radius: 4px; overflow-x: auto; margin: 10px 0; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            return sb.ToString();
        }

        public static string GenerateErrorReport(string sqlQuery, string errorMessage)
        {
            var sb = new StringBuilder();
            sb.Append(GetHtmlHeader());
            sb.AppendLine("<body>");            
            sb.AppendLine("<h1>SQL Query Analysis</h1>");

            // Error Message (shown first)
            sb.AppendLine("<div class=\"error\">");
            sb.AppendLine("<h2>⚠️ Error Occurred</h2>");
            sb.AppendLine($"<pre>{errorMessage}</pre>");
            sb.AppendLine("</div>");
            
            // Original Query
            sb.AppendLine("<h2>Query</h2>");
            sb.AppendLine($"<div class=\"query\"><pre>{sqlQuery}</pre></div>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public static string GenerateReport(string originalQuery, DataTable metrics, List<OptimizationSuggestion> suggestions)
        {
            var sb = new StringBuilder();
            sb.Append(GetHtmlHeader());
            sb.AppendLine("<body>");

            sb.AppendLine("<h1>SQL Query Optimization Report</h1>");            // Original Query
            sb.AppendLine("<h2>Original Query</h2>");
            sb.AppendLine($"<div class=\"query\"><pre>{originalQuery}</pre></div>");

            // Optimization Suggestions (moved up)
            sb.AppendLine("<h2>Optimization Suggestions</h2>");
            sb.AppendLine("<div class=\"suggestions\">");

            if (suggestions.Count == 0)
            {
                sb.AppendLine("<p>No significant optimization opportunities detected.</p>");
            }
            else
            {
                foreach (var suggestion in suggestions.OrderByDescending(s => s.Impact))
                {
                    var impactClass = suggestion.Impact.ToString().ToLower() + "-impact";
                    var impactLabel = suggestion.Impact.ToString() + " impact";
                    var labelClass = suggestion.Impact.ToString().ToLower() + "-label";

                    sb.AppendLine($"<div class=\"suggestion {impactClass}\">");
                    sb.AppendLine($"<span class=\"impact-label {labelClass}\">{impactLabel}</span>");
                    sb.AppendLine($"<h3>{suggestion.Type}: {suggestion.ObjectName}</h3>");
                    sb.AppendLine($"<p>{suggestion.Description}</p>");
                    if (!string.IsNullOrEmpty(suggestion.RecommendedAction))
                    {
                        sb.AppendLine($"<pre>{suggestion.RecommendedAction}</pre>");
                    }
                    sb.AppendLine("</div>");
                }
            }
            sb.AppendLine("</div>");            // Performance Metrics (moved to end)  
            sb.AppendLine("<div class=\"metrics\">");
            
            if (metrics == null || metrics.Rows.Count == 0)
            {
                sb.AppendLine("<p>No performance metrics available.</p>");
            }
            else
            {
                // Add a performance summary before the detailed metrics
                sb.AppendLine("<h2>Performance Summary</h2>");
                sb.AppendLine("<p>This section shows the performance characteristics of your SQL query. These metrics can help identify bottlenecks and areas for optimization.</p>");
                
                sb.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\">");
                sb.AppendLine("<tr><th>Metric</th><th>Value</th></tr>");
                  // Display metrics in the new format with MetricName, MetricValue, MetricUnit columns
                foreach (DataRow row in metrics.Rows)
                {
                    string metricName = row["MetricName"].ToString();
                    string metricValue = FormatMetricValue(row["MetricValue"], row["MetricUnit"].ToString());
                    string displayName = FormatMetricDisplayName(metricName, row["MetricUnit"].ToString());
                    
                    AddMetricRow(sb, displayName, metricValue);
                }
                
                sb.AppendLine("</table>");
                
                // Add explanations for the metrics
                sb.AppendLine("<div style=\"margin-top: 15px; font-size: 0.9em; color: #555;\">");
                sb.AppendLine("<h4>Understanding These Metrics</h4>");
                sb.AppendLine("<ul>");
                sb.AppendLine("<li><strong>CPU Time</strong>: The amount of processor time used by the query.</li>");
                sb.AppendLine("<li><strong>Elapsed Time</strong>: The total wall-clock time taken by the query.</li>");
                sb.AppendLine("<li><strong>Logical Reads</strong>: The number of pages read from the buffer cache (memory).</li>");
                sb.AppendLine("<li><strong>Logical Writes</strong>: The number of pages written to the buffer cache.</li>");
                sb.AppendLine("<li><strong>Rows</strong>: The number of rows processed/returned by the query.</li>");
                sb.AppendLine("</ul>");
                sb.AppendLine("<p>High logical reads, CPU time, or elapsed time may indicate optimization opportunities.</p>");
                sb.AppendLine("</div>");
            }
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static string FormatMetricValue(object value, string unit)
        {
            if (value == null || value == DBNull.Value)
            {
                return "N/A";
            }
            
            // Format the value based on its unit
            switch (unit.ToLower())
            {
                case "microseconds":
                    // Convert microseconds to milliseconds for better readability
                    if (decimal.TryParse(value.ToString(), out decimal microseconds))
                    {
                        return $"{microseconds / 1000:N2} ms";
                    }
                    break;
                
                case "pages":
                    return $"{value:N0}";
                
                case "count":
                    return $"{value:N0}";
                
                default:
                    return value.ToString();
            }
            
            return value.ToString();
        }
        
        private static string FormatMetricDisplayName(string metricName, string unit)
        {
            // Format the display name based on the metric and its unit
            switch (metricName)
            {
                case "CPU Time":
                    return "CPU Time";
                
                case "Elapsed Time":
                    return "Elapsed Time";
                
                case "Logical Reads":
                    return "Logical Reads (Buffer Pages)";
                
                case "Logical Writes":
                    return "Logical Writes (Buffer Pages)";
                
                case "Rows Returned":
                    return "Rows Returned";
                
                case "Execution Count":
                    return "Execution Count";
                
                default:
                    return metricName;
            }
        }
    }
}
