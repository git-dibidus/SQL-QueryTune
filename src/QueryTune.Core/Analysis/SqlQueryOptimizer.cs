using System.Collections.Generic;
using QueryTune.Core.Models;
using QueryTune.Core.Reporting;
using System.Data;

namespace QueryTune.Core.Analysis
{
    public class SqlQueryOptimizer
    {
        private readonly QueryAnalyzer _queryAnalyzer;
        private readonly ExecutionPlanAnalyzer _planAnalyzer;
        private readonly IndexRecommender _indexRecommender;
        private readonly StatisticsAnalyzer _statsAnalyzer;
        private readonly HtmlReportGenerator _reportGenerator;

        public SqlQueryOptimizer(string connectionString)
        {
            _queryAnalyzer = new QueryAnalyzer(connectionString);
            _planAnalyzer = new ExecutionPlanAnalyzer();
            _indexRecommender = new IndexRecommender();
            _statsAnalyzer = new StatisticsAnalyzer(connectionString);
            _reportGenerator = new HtmlReportGenerator();
        }

        private DataTable CreateDefaultMetricsTable()
        {
            // Create a default metrics table with placeholder values
            var defaultMetrics = new DataTable();
            defaultMetrics.Columns.Add("MetricName", typeof(string));
            defaultMetrics.Columns.Add("MetricValue", typeof(string));
            defaultMetrics.Columns.Add("MetricUnit", typeof(string));
            
            defaultMetrics.Rows.Add("Execution Count", "1", "count");
            defaultMetrics.Rows.Add("CPU Time", "N/A", "microseconds");
            defaultMetrics.Rows.Add("Elapsed Time", "N/A", "microseconds");
            defaultMetrics.Rows.Add("Logical Reads", "N/A", "pages");
            defaultMetrics.Rows.Add("Logical Writes", "N/A", "pages");
            defaultMetrics.Rows.Add("Rows Returned", "N/A", "count");
            
            return defaultMetrics;
        }

        public string AnalyzeAndOptimize(string sqlQuery)
        {            // Step 1: Get execution plan
            var plan = _queryAnalyzer.GetExecutionPlan(sqlQuery);

            // Step 2: Get performance metrics
            var metrics = _queryAnalyzer.GetQueryMetrics(sqlQuery);
            
            // Make sure we have metrics
            if (metrics == null || metrics.Rows.Count == 0)
            {
                metrics = CreateDefaultMetricsTable();
            }

            // Step 3: Analyze execution plan
            var planSuggestions = ExecutionPlanAnalyzer.AnalyzePlan(plan);

            // Step 4: Recommend indexes
            var indexSuggestions = IndexRecommender.RecommendIndexes(plan, metrics);

            // Step 5: Check statistics
            var statsSuggestions = _statsAnalyzer.CheckStatistics(plan);

            // Combine all suggestions
            var allSuggestions = new List<OptimizationSuggestion>();
            allSuggestions.AddRange(planSuggestions);
            allSuggestions.AddRange(indexSuggestions);
            allSuggestions.AddRange(statsSuggestions);

            // Generate report
            return HtmlReportGenerator.GenerateReport(sqlQuery, metrics, allSuggestions);
        }
    }
}
