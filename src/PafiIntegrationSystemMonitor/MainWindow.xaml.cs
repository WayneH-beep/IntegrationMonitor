using System.Windows;
using PafiIntegrationSystemMonitor.Models;
using PafiIntegrationSystemMonitor.ViewModels;

namespace PafiIntegrationSystemMonitor;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        await _viewModel.ShutdownAsync();
    }

    private void AddGroup_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ConfigViewModel?.Groups.Add(new IntegrationGroupDefinition { Name = "New Integration", HealthRule = GroupHealthRuleType.CriticalWithRoleAwareness });
    }
}
