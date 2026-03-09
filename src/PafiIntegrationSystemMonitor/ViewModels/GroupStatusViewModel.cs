using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.ViewModels;

public sealed class GroupStatusViewModel : ViewModelBase
{
    public string GroupName { get; }

    private MonitorStatus _status;
    private string _detail;

    public GroupStatusViewModel(string groupName)
    {
        GroupName = groupName;
        _status = MonitorStatus.Unknown;
        _detail = "Awaiting checks";
    }

    public MonitorStatus Status { get => _status; set => SetProperty(ref _status, value); }
    public string Detail { get => _detail; set => SetProperty(ref _detail, value); }
}
