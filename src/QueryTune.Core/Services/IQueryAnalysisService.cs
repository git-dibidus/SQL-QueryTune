using System.Threading.Tasks;

namespace QueryTune.Core.Services
{
    public class AnalysisResult
    {
        public string HtmlReport { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IQueryAnalysisService
    {
        Task<AnalysisResult> AnalyzeQueryAsync(string connectionString, string sqlQuery);
    }
}
