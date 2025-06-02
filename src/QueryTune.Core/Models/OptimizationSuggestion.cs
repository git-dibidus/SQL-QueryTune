namespace QueryTune.Core.Models
{
    public class OptimizationSuggestion
    {
        public SuggestionType Type { get; set; }
        public string Description { get; set; }
        public string ObjectName { get; set; }
        public SuggestionImpact Impact { get; set; }
        public string RecommendedAction { get; set; }
    }

    public enum SuggestionType
    {
        CreateIndex,
        IncludeColumns,
        UpdateStatistics,
        QueryRewrite,
        Other
    }

    public enum SuggestionImpact
    {
        Low,
        Medium,
        High
    }
}
