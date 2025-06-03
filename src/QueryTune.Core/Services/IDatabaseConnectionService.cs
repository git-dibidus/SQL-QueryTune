using QueryTune.Core.Models;
using System.Threading.Tasks;

namespace QueryTune.Core.Services
{
    public interface IDatabaseConnectionService
    {
        Task<bool> TestConnectionAsync(ConnectionParameters parameters);
        string GetConnectionString(ConnectionParameters parameters);
    }
}
