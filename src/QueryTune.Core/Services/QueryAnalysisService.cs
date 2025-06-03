using System;
using System.Threading.Tasks;

namespace QueryTune.Core.Services
{
    public class QueryAnalysisService : IQueryAnalysisService
    {
        public async Task<AnalysisResult> AnalyzeQueryAsync(string connectionString, string sqlQuery)
        {
            try
            {
                var optimizer = new SqlQueryOptimizer(connectionString);
                string report = await Task.Run(() => optimizer.AnalyzeAndOptimize(sqlQuery));
                
                return new AnalysisResult
                {
                    HtmlReport = report,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new AnalysisResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
