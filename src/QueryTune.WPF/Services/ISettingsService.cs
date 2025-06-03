using QueryTune.Core.Models;

namespace QueryTune.WPF.Services
{    
    public interface ISettingsService
    {
        Task SaveConnectionParametersAsync(ConnectionParameters parameters);
        Task<ConnectionParameters> LoadConnectionParametersAsync();
        Task SaveLastQueryAsync(string query);
        Task<string> LoadLastQueryAsync();
    }
}
