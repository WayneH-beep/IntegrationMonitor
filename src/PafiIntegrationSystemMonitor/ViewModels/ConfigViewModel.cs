using System.Collections.ObjectModel;
using System.Windows.Input;
using PafiIntegrationSystemMonitor.Commands;
using PafiIntegrationSystemMonitor.Models;
using PafiIntegrationSystemMonitor.Services;

namespace PafiIntegrationSystemMonitor.ViewModels;

public sealed class ConfigViewModel : ViewModelBase
{
    private readonly ConfigurationService _configurationService;
    private readonly ServiceDiscoveryService _serviceDiscoveryService;

    private string _serviceSearchText = string.Empty;
    private string _manualServiceName = string.Empty;

    public ConfigViewModel(AppConfiguration configuration, ConfigurationService configurationService, ServiceDiscoveryService serviceDiscoveryService)
    {
        Configuration = configuration;
        _configurationService = configurationService;
        _serviceDiscoveryService = serviceDiscoveryService;

        Items = new ObservableCollection<MonitoredItemDefinition>(configuration.Items);
        Groups = new ObservableCollection<IntegrationGroupDefinition>(configuration.Groups);
        AvailableServices = new ObservableCollection<string>();

        RefreshServicesCommand = new AsyncRelayCommand(RefreshServicesAsync);
        AddManualServiceCommand = new AsyncRelayCommand(AddManualServiceAsync);
        AddSelectedServiceCommand = new AsyncRelayCommand(AddSelectedServiceAsync, () => !string.IsNullOrWhiteSpace(SelectedDiscoveredService));
        AddDeviceCommand = new AsyncRelayCommand(AddDeviceAsync);
        AddTcpEndpointCommand = new AsyncRelayCommand(AddTcpEndpointAsync);
        DeleteItemCommand = new AsyncRelayCommand(DeleteSelectedItemAsync, () => SelectedItem != null);
        MoveUpCommand = new AsyncRelayCommand(MoveUpAsync, () => SelectedItem != null);
        MoveDownCommand = new AsyncRelayCommand(MoveDownAsync, () => SelectedItem != null);
        SaveCommand = new AsyncRelayCommand(SaveAsync);

        _ = RefreshServicesAsync();
    }

    public AppConfiguration Configuration { get; }
    public ObservableCollection<MonitoredItemDefinition> Items { get; }
    public ObservableCollection<IntegrationGroupDefinition> Groups { get; }
    public ObservableCollection<string> AvailableServices { get; }

    public string ServiceSearchText
    {
        get => _serviceSearchText;
        set
        {
            if (!SetProperty(ref _serviceSearchText, value)) return;
            _ = RefreshServicesAsync();
        }
    }

    public string ManualServiceName { get => _manualServiceName; set => SetProperty(ref _manualServiceName, value); }
    public string? SelectedDiscoveredService { get; set; }
    public MonitoredItemDefinition? SelectedItem { get; set; }

    public ICommand RefreshServicesCommand { get; }
    public ICommand AddManualServiceCommand { get; }
    public ICommand AddSelectedServiceCommand { get; }
    public ICommand AddDeviceCommand { get; }
    public ICommand AddTcpEndpointCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand SaveCommand { get; }

    public async Task SaveAsync()
    {
        Configuration.Items = Items.ToList();
        Configuration.Groups = Groups.ToList();
        await _configurationService.SaveAsync(Configuration);
    }

    private Task AddDeviceAsync()
    {
        Items.Add(new MonitoredItemDefinition
        {
            FriendlyName = "New Device",
            Type = MonitoredItemType.Device,
            Target = "127.0.0.1",
            GroupName = Groups.FirstOrDefault()?.Name ?? "Default",
            IsCritical = false,
        });
        return Task.CompletedTask;
    }

    private Task AddTcpEndpointAsync()
    {
        Items.Add(new MonitoredItemDefinition
        {
            FriendlyName = "New TCP Endpoint",
            Type = MonitoredItemType.TcpEndpoint,
            Target = "127.0.0.1",
            Port = 80,
            GroupName = Groups.FirstOrDefault()?.Name ?? "Default",
        });
        return Task.CompletedTask;
    }

    private Task AddManualServiceAsync()
    {
        if (string.IsNullOrWhiteSpace(ManualServiceName)) return Task.CompletedTask;

        Items.Add(new MonitoredItemDefinition
        {
            FriendlyName = ManualServiceName,
            Type = MonitoredItemType.Service,
            Target = ManualServiceName,
            GroupName = Groups.FirstOrDefault()?.Name ?? "Default",
        });
        ManualServiceName = string.Empty;
        return Task.CompletedTask;
    }

    private Task AddSelectedServiceAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedDiscoveredService)) return Task.CompletedTask;

        Items.Add(new MonitoredItemDefinition
        {
            FriendlyName = SelectedDiscoveredService,
            Type = MonitoredItemType.Service,
            Target = SelectedDiscoveredService,
            GroupName = Groups.FirstOrDefault()?.Name ?? "Default",
        });
        return Task.CompletedTask;
    }

    private async Task RefreshServicesAsync()
    {
        await Task.Run(() =>
        {
            var services = _serviceDiscoveryService.GetAllServiceNames(ServiceSearchText);
            App.Current.Dispatcher.Invoke(() =>
            {
                AvailableServices.Clear();
                foreach (var service in services)
                {
                    AvailableServices.Add(service);
                }
            });
        });
    }

    private Task DeleteSelectedItemAsync()
    {
        if (SelectedItem != null)
        {
            Items.Remove(SelectedItem);
        }
        return Task.CompletedTask;
    }

    private Task MoveUpAsync()
    {
        if (SelectedItem == null) return Task.CompletedTask;
        var index = Items.IndexOf(SelectedItem);
        if (index > 0) Items.Move(index, index - 1);
        return Task.CompletedTask;
    }

    private Task MoveDownAsync()
    {
        if (SelectedItem == null) return Task.CompletedTask;
        var index = Items.IndexOf(SelectedItem);
        if (index >= 0 && index < Items.Count - 1) Items.Move(index, index + 1);
        return Task.CompletedTask;
    }
}
