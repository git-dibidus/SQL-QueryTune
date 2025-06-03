using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;
using QueryTune.WPF.ViewModels;
using System.ComponentModel;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace QueryTune.WPF.Views
{    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private IHighlightingDefinition? _sqlHighlighting;

        public MainWindow()
        {
            InitializeComponent();
            LoadSqlHighlighting();
            Loaded += MainWindow_Loaded;
        }        private void LoadSqlHighlighting()
        {
            try
            {
                // Load the syntax highlighting definition from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("QueryTune.WPF.Assets.SQL.xshd");
                if (stream != null)
                {
                    using var reader = new XmlTextReader(stream);
                    _sqlHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    HighlightingManager.Instance.RegisterHighlighting("SQL", new[] { ".sql" }, _sqlHighlighting);
                    
                    // Apply the highlighting to the editor
                    if (QueryEditor != null)
                    {
                        QueryEditor.SyntaxHighlighting = _sqlHighlighting;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading SQL syntax highlighting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (MainViewModel)DataContext;

            // Set up WebView2 for results
            ResultsViewer.NavigationCompleted += (s, e) =>
            {
                if (_viewModel != null && !string.IsNullOrEmpty(_viewModel.AnalysisResults))
                {
                    ResultsViewer.NavigateToString(_viewModel.AnalysisResults);
                }
            };

            // Subscribe to analysis results changes
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.AnalysisResults) && _viewModel != null)
            {
                ResultsViewer.NavigateToString(_viewModel.AnalysisResults);
            }
        }
    }
}