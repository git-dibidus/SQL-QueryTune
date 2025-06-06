﻿using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using QueryTune.WPF.ViewModels;

namespace QueryTune.WPF.Views
{    
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private IHighlightingDefinition? _sqlHighlighting;
        private bool _isSettingPassword = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadSqlHighlighting();
            Loaded += MainWindow_Loaded;
        }        
        
        private void LoadSqlHighlighting()
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
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MainViewModel;

            if (_viewModel != null)
            {
                // Setup PasswordBox
                PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                // Load saved settings
                await _viewModel.LoadAsync();

                // Set initial password if exists
                if (!string.IsNullOrEmpty(_viewModel.ConnectionParameters?.Password))
                {
                    _isSettingPassword = true;
                    PasswordBox.Password = _viewModel.ConnectionParameters.Password;
                    _isSettingPassword = false;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.ConnectionParameters != null && !_isSettingPassword)
            {
                _viewModel.ConnectionParameters.Password = PasswordBox.Password;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Clear password box when switching to Windows Authentication
            if (e.PropertyName == nameof(MainViewModel.ConnectionParameters))
            {
                if (_viewModel?.ConnectionParameters != null)
                {
                    if (_viewModel.ConnectionParameters.UseWindowsAuthentication)
                    {
                        PasswordBox.Password = string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(_viewModel.ConnectionParameters.Password))
                    {
                        _isSettingPassword = true;
                        PasswordBox.Password = _viewModel.ConnectionParameters.Password;
                        _isSettingPassword = false;
                    }
                }
            }

            if (e.PropertyName == nameof(MainViewModel.AnalysisResults) && _viewModel != null)
            {
                ResultsViewer.NavigateToString(_viewModel.AnalysisResults);
            }
        }
    }
}