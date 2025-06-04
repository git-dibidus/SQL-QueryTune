using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QueryTune.Core.Models;
using QueryTune.Core.Reporting;
using QueryTune.Core.Services;
using QueryTune.WPF.Helpers;
using QueryTune.WPF.Services;

namespace QueryTune.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDatabaseConnectionService _connectionService;
        private readonly IQueryAnalysisService _analysisService;
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private ConnectionParameters connectionParameters;
        
        [ObservableProperty]
        private string sqlQuery = string.Empty;

        [ObservableProperty]
        private string analysisResults = string.Empty;

        [ObservableProperty]
        private bool isConnecting;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private string connectionError = string.Empty;

        [ObservableProperty]
        private string formTitle = string.Empty;

        public MainViewModel(
            IDatabaseConnectionService connectionService,
            IQueryAnalysisService analysisService,
            ISettingsService settingsService)
        {
            _connectionService = connectionService;
            _analysisService = analysisService;
            _settingsService = settingsService;
            ConnectionParameters = new ConnectionParameters();

            FormTitle = $"SQL QueryTune (V {AppVersionHelper.InformationalVersion})";

            // Note: In constructor we initialize with default values
            // The actual loading happens in LoadAsync which should be called by the View
        }

        public async Task LoadAsync()
        {
            await LoadConnectionParametersAsync();
            await LoadLastQueryAsync();
        }

        private async Task LoadConnectionParametersAsync()
        {
            try
            {
                ConnectionParameters = await _settingsService.LoadConnectionParametersAsync();
            }
            catch (Exception ex)
            {
                ConnectionParameters = new ConnectionParameters();
                StatusMessage = $"Failed to load saved connection: {ex.Message}";
            }
        }

        private async Task SaveConnectionParametersAsync()
        {
            try
            {
                await _settingsService.SaveConnectionParametersAsync(ConnectionParameters);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save connection: {ex.Message}";
            }
        }

        private async Task LoadLastQueryAsync()
        {
            try
            {
                SqlQuery = await _settingsService.LoadLastQueryAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load last query: {ex.Message}";
            }
        }

        private async Task SaveLastQueryAsync()
        {
            try
            {
                await _settingsService.SaveLastQueryAsync(SqlQuery);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save query: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task TestConnection()
        {
            if (string.IsNullOrWhiteSpace(ConnectionParameters?.ServerName))
            {
                StatusMessage = "Please enter a server name";
                return;
            }

            try
            {
                // Clear any previous error
                ConnectionError = string.Empty;
                IsConnecting = true;
                StatusMessage = "Testing connection...";                
                
                var success = await _connectionService.TestConnectionAsync(ConnectionParameters);

                if (success)
                {
                    StatusMessage = "Connection successful";
                    await SaveConnectionParametersAsync();
                }
                else
                {
                    StatusMessage = "Connection failed";
                    ConnectionError = "The connection test failed. Please verify your connection settings and try again.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Connection failed";
                ConnectionError = $"Connection Error Details:\n{ex.Message}";
                if (ex.InnerException != null)
                {
                    ConnectionError += $"\n\nAdditional Information:\n{ex.InnerException.Message}";
                }
            }
            finally
            {
                IsConnecting = false;
            }
        }

        [RelayCommand]
        private async Task RunAnalysis()
        {
            if (string.IsNullOrWhiteSpace(SqlQuery))
            {
                StatusMessage = "Please enter a SQL query";
                return;
            }  
            
            try
            {
                // Clear previous results before starting new analysis
                AnalysisResults = string.Empty;
                StatusMessage = "Analyzing query...";
                await SaveLastQueryAsync();

                var connectionString = _connectionService.GetConnectionString(ConnectionParameters);
                var result = await _analysisService.AnalyzeQueryAsync(connectionString, SqlQuery);

                if (result.IsSuccess)
                {
                    AnalysisResults = result.HtmlReport;
                    StatusMessage = "Analysis complete";
                }
                else
                {
                    // Show error in the results panel
                    AnalysisResults = HtmlReportGenerator.GenerateErrorReport(SqlQuery, result.ErrorMessage);
                    StatusMessage = "Analysis completed with errors";
                }
            }
            catch (Exception ex)
            {
                // Show any unexpected errors in the results panel
                AnalysisResults = HtmlReportGenerator.GenerateErrorReport(SqlQuery, ex.Message);
                StatusMessage = "Analysis completed with errors";
            }
        }
    }
}
