using Microsoft.Extensions.DependencyInjection;
using QueryTune.Core.Services;
using QueryTune.WPF.Services;
using QueryTune.WPF.ViewModels;
using QueryTune.WPF.Views;
using System.Windows;

namespace QueryTune.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }        
        
        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
            services.AddSingleton<IQueryAnalysisService, QueryAnalysisService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            // Register ViewModels
            services.AddTransient<MainViewModel>();

            // Register Views
            services.AddTransient(sp => new MainWindow
            {
                DataContext = sp.GetRequiredService<MainViewModel>()
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
