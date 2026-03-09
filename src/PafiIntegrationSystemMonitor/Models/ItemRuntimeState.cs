namespace PafiIntegrationSystemMonitor.Models;

public sealed class ItemRuntimeState
{
    public Guid ItemId { get; init; }
    public MonitorStatus Status { get; set; } = MonitorStatus.Unknown;
    public DateTime LastCheckedUtc { get; set; }
    public DateTime LastChangedUtc { get; set; }
    public string Detail { get; set; } = "Not checked yet";
    public long? ResponseTimeMs { get; set; }
}
