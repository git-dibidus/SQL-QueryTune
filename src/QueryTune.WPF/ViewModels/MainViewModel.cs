using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QueryTune.Core.Models;
using QueryTune.Core.Services;
using QueryTune.WPF.Services;
using System;
using System.Threading.Tasks;

namespace QueryTune.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDatabaseConnectionService _connectionService;
        private readonly IQueryAnalysisService _analysisService;
        private readonly ISettingsService _settingsService;        
        
        public MainViewModel(
            IDatabaseConnectionService connectionService,
            IQueryAnalysisService analysisService,
            ISettingsService settingsService)
        {
            _connectionService = connectionService;
            _analysisService = analysisService;
            _settingsService = settingsService;
            ConnectionParameters = new ConnectionParameters();
            
            // Note: In constructor we initialize with default values
            // The actual loading happens in LoadAsync which should be called by the View
        }        public async Task LoadAsync()
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

        [ObservableProperty]
        private ConnectionParameters connectionParameters;

        //partial void OnConnectionParametersChanged(ConnectionParameters value)
        //{
        //    // Save connection parameters whenever they change
        //    if (value != null)
        //    {
        //        SaveConnectionParametersAsync().ConfigureAwait(false);
        //    }
        //}

        [ObservableProperty]
        private string sqlQuery = string.Empty;

        [ObservableProperty]
        private string analysisResults = string.Empty;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private bool isConnecting;

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
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Connection error: {ex.Message}";
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
            }            try
            {
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
                    StatusMessage = $"Analysis failed: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Analysis error: {ex.Message}";
            }
        }
    }
}
