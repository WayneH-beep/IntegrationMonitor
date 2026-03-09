namespace PafiIntegrationSystemMonitor.Models;

public sealed class HistoryEntry
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}
