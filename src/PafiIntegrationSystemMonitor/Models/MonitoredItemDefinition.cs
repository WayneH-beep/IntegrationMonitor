namespace PafiIntegrationSystemMonitor.Models;

public sealed class MonitoredItemDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FriendlyName { get; set; } = string.Empty;
    public MonitoredItemType Type { get; set; }
    public string Target { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsCritical { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public string GroupName { get; set; } = "Default";
    public EndpointRole Role { get; set; } = EndpointRole.Standalone;
    public string PairKey { get; set; } = string.Empty;
    public int PollIntervalSeconds { get; set; } = 0;
    public int TimeoutMilliseconds { get; set; } = 0;
}
