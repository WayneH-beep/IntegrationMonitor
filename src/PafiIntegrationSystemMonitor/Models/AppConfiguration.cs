namespace PafiIntegrationSystemMonitor.Models;

public sealed class AppConfiguration
{
    public int DefaultPollIntervalSeconds { get; set; } = 10;
    public int DefaultTimeoutMilliseconds { get; set; } = 3000;
    public bool StartMinimizedToTray { get; set; }
    public bool EnableDesktopNotifications { get; set; } = true;
    public List<MonitoredItemDefinition> Items { get; set; } = new();
    public List<IntegrationGroupDefinition> Groups { get; set; } = new();
}
