using System.Collections.Generic;
using QueryTune.Core.Analysis;
using QueryTune.Core.Models;
using QueryTune.Core.Reporting;

namespace QueryTune.Core
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

        public string AnalyzeAndOptimize(string sqlQuery)
        {
            // Step 1: Get execution plan
            var plan = _queryAnalyzer.GetExecutionPlan(sqlQuery);

            // Step 2: Get performance metrics
            var metrics = _queryAnalyzer.GetQueryMetrics(sqlQuery);

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
