using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PafiIntegrationSystemMonitor.Commands;
using PafiIntegrationSystemMonitor.Models;
using PafiIntegrationSystemMonitor.Services;

namespace PafiIntegrationSystemMonitor.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly ConfigurationService _configurationService;
    private readonly HistoryService _historyService;
    private readonly MonitoringEngine _monitoringEngine;
    private readonly GroupHealthService _groupHealthService;
    private readonly ServiceControlService _serviceControlService;

    private AppConfiguration _configuration = new();
    private StatusItemViewModel? _selectedStatusItem;

    public MainWindowViewModel()
    {
        _configurationService = new ConfigurationService();
        _historyService = new HistoryService();
        _groupHealthService = new GroupHealthService();
        _serviceControlService = new ServiceControlService();

        _monitoringEngine = new MonitoringEngine(new MonitorProbeService(), _historyService);
        _monitoringEngine.ItemUpdated += OnItemUpdated;

        StatusItems = new ObservableCollection<StatusItemViewModel>();
        GroupStatuses = new ObservableCollection<GroupStatusViewModel>();

        RefreshNowCommand = new AsyncRelayCommand(RefreshNowAsync);
        RestartSelectedServiceCommand = new AsyncRelayCommand(RestartSelectedServiceAsync, CanRestartSelectedService);
    }

    public ConfigViewModel? ConfigViewModel { get; private set; }
    public HistoryViewModel? HistoryViewModel { get; private set; }

    public ObservableCollection<StatusItemViewModel> StatusItems { get; }
    public ObservableCollection<GroupStatusViewModel> GroupStatuses { get; }

    public StatusItemViewModel? SelectedStatusItem
    {
        get => _selectedStatusItem;
        set
        {
            SetProperty(ref _selectedStatusItem, value);
            Raise(nameof(CanRestartService));
        }
    }

    public ICommand RefreshNowCommand { get; }
    public ICommand RestartSelectedServiceCommand { get; }

    public int TotalItems => StatusItems.Count;
    public int HealthyCount => StatusItems.Count(i => i.Status == MonitorStatus.Healthy);
    public int WarningCount => StatusItems.Count(i => i.Status == MonitorStatus.Warning);
    public int FailedCount => StatusItems.Count(i => i.Status == MonitorStatus.Failed);
    public int UnknownCount => StatusItems.Count(i => i.Status == MonitorStatus.Unknown);
    public bool CanRestartService => CanRestartSelectedService();

    public async Task InitializeAsync()
    {
        _configuration = await _configurationService.LoadAsync();
        await _historyService.InitializeAsync();

        ConfigViewModel = new ConfigViewModel(_configuration, _configurationService, new ServiceDiscoveryService());
        HistoryViewModel = new HistoryViewModel(_historyService);
        await HistoryViewModel.RefreshAsync();

        StatusItems.Clear();
        foreach (var item in _configuration.Items)
        {
            StatusItems.Add(new StatusItemViewModel(item));
        }

        _monitoringEngine.Start(_configuration);
        Raise(nameof(ConfigViewModel));
        Raise(nameof(HistoryViewModel));
    }

    public async Task ShutdownAsync()
    {
        _monitoringEngine.Stop();
        await _monitoringEngine.DisposeAsync();
    }

    private async Task RefreshNowAsync()
    {
        foreach (var item in _configuration.Items.Where(i => i.IsEnabled))
        {
            await _monitoringEngine.CheckNowAsync(item, _configuration);
        }
    }

    private void OnItemUpdated(MonitoredItemDefinition definition, ItemRuntimeState state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = StatusItems.First(x => x.Definition.Id == definition.Id);
            vm.Status = state.Status;
            vm.LastChecked = state.LastCheckedUtc.ToLocalTime();
            vm.LastChanged = state.LastChangedUtc.ToLocalTime();
            vm.Detail = state.Detail;

            RecomputeGroupHealth();
            Raise(nameof(TotalItems));
            Raise(nameof(HealthyCount));
            Raise(nameof(WarningCount));
            Raise(nameof(FailedCount));
            Raise(nameof(UnknownCount));
            Raise(nameof(CanRestartService));
        });
    }

    private void RecomputeGroupHealth()
    {
        var latest = _groupHealthService.Compute(_configuration, _monitoringEngine.States);
        GroupStatuses.Clear();
        foreach (var group in latest)
        {
            GroupStatuses.Add(new GroupStatusViewModel(group.GroupName)
            {
                Status = group.Status,
                Detail = group.Detail,
            });
        }
    }

    private bool CanRestartSelectedService()
    {
        return SelectedStatusItem is { Definition.Type: MonitoredItemType.Service } selected
               && _serviceControlService.CanControlService(selected.Definition.Target);
    }

    private async Task RestartSelectedServiceAsync()
    {
        if (SelectedStatusItem is not { Definition.Type: MonitoredItemType.Service } selected) return;

        var confirmation = MessageBox.Show(
            $"Restart service '{selected.Definition.Target}'?",
            "Confirm restart",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes) return;

        var result = await _serviceControlService.RestartAsync(selected.Definition.Target);
        MessageBox.Show(result.detail, result.success ? "Restart completed" : "Restart failed", MessageBoxButton.OK,
            result.success ? MessageBoxImage.Information : MessageBoxImage.Error);
    }
}
