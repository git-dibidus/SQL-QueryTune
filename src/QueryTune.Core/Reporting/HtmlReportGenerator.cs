using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using QueryTune.Core.Models;

namespace QueryTune.Core.Reporting
{
    public class HtmlReportGenerator
    {
        public static string GenerateReport(string originalQuery, DataTable metrics, List<OptimizationSuggestion> suggestions)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>SQL Query Optimization Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("h1 { color: #2a5885; }");
            sb.AppendLine(".query { background: #f5f5f5; padding: 10px; border-radius: 5px; }");
            sb.AppendLine(".metrics { margin: 20px 0; }");
            sb.AppendLine(".suggestions { margin-top: 30px; }");
            sb.AppendLine(".suggestion { margin-bottom: 15px; padding: 10px; border-left: 4px solid; }");
            sb.AppendLine(".high-impact { border-color: #e74c3c; background: #fde8e6; }");
            sb.AppendLine(".medium-impact { border-color: #f39c12; background: #fef5e6; }");
            sb.AppendLine(".low-impact { border-color: #3498db; background: #eaf2f8; }");
            sb.AppendLine(".impact-label { display: inline-block; padding: 2px 5px; border-radius: 3px; color: white; font-size: 0.8em; }");
            sb.AppendLine(".high-label { background: #e74c3c; }");
            sb.AppendLine(".medium-label { background: #f39c12; }");
            sb.AppendLine(".low-label { background: #3498db; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<h1>SQL Query Optimization Report</h1>");

            // Original Query
            sb.AppendLine("<h2>Original Query</h2>");
            sb.AppendLine($"<div class=\"query\"><pre>{originalQuery}</pre></div>");

            // Performance Metrics
            sb.AppendLine("<h2>Performance Metrics</h2>");
            sb.AppendLine("<div class=\"metrics\">");
            sb.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"0\">");
            sb.AppendLine("<tr>");
            foreach (DataColumn col in metrics.Columns)
            {
                sb.AppendLine($"<th>{col.ColumnName}</th>");
            }
            sb.AppendLine("</tr>");

            foreach (DataRow row in metrics.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (var item in row.ItemArray)
                {
                    sb.AppendLine($"<td>{item}</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Optimization Suggestions
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

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
