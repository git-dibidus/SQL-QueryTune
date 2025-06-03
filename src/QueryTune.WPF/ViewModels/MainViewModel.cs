using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QueryTune.Core.Models;
using QueryTune.Core.Services;
using System;
using System.Threading.Tasks;

namespace QueryTune.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDatabaseConnectionService _connectionService;
        private readonly IQueryAnalysisService _analysisService;

        public MainViewModel(IDatabaseConnectionService connectionService, IQueryAnalysisService analysisService)
        {
            _connectionService = connectionService;
            _analysisService = analysisService;
            ConnectionParameters = new ConnectionParameters();
        }

        [ObservableProperty]
        private ConnectionParameters connectionParameters;

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

                StatusMessage = success ? "Connection successful" : "Connection failed";
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
            }

            try
            {
                StatusMessage = "Analyzing query...";

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
