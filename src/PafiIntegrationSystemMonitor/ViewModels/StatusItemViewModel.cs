using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.ViewModels;

public sealed class StatusItemViewModel : ViewModelBase
{
    public MonitoredItemDefinition Definition { get; }

    private MonitorStatus _status;
    private string _detail = "Not checked yet";
    private DateTime _lastChecked;
    private DateTime _lastChanged;

    public StatusItemViewModel(MonitoredItemDefinition definition)
    {
        Definition = definition;
        _status = MonitorStatus.Unknown;
    }

    public string FriendlyName => Definition.FriendlyName;
    public string ItemType => Definition.Type.ToString();
    public string GroupName => Definition.GroupName;
    public string Role => Definition.Role.ToString();
    public MonitorStatus Status { get => _status; set => SetProperty(ref _status, value); }
    public DateTime LastChecked { get => _lastChecked; set => SetProperty(ref _lastChecked, value); }
    public DateTime LastChanged { get => _lastChanged; set => SetProperty(ref _lastChanged, value); }
    public string Detail { get => _detail; set => SetProperty(ref _detail, value); }
}
