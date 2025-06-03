using QueryTune.Core.Models;
using System.Threading.Tasks;

namespace QueryTune.WPF.Services
{    public interface ISettingsService
    {
        Task SaveConnectionParametersAsync(ConnectionParameters parameters);
        Task<ConnectionParameters> LoadConnectionParametersAsync();
        Task SaveLastQueryAsync(string query);
        Task<string> LoadLastQueryAsync();
    }
}
