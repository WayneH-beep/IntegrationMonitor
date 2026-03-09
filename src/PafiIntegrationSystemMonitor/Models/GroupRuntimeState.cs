namespace PafiIntegrationSystemMonitor.Models;

public sealed class GroupRuntimeState
{
    public string GroupName { get; init; } = string.Empty;
    public MonitorStatus Status { get; init; } = MonitorStatus.Unknown;
    public string Detail { get; init; } = string.Empty;
}
