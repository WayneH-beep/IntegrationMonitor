namespace PafiIntegrationSystemMonitor.Models;

public enum MonitoredItemType
{
    Service,
    Device,
    TcpEndpoint
}

public enum MonitorStatus
{
    Unknown,
    Healthy,
    Warning,
    Failed
}

public enum EndpointRole
{
    Standalone,
    Primary,
    Secondary
}

public enum GroupHealthRuleType
{
    CriticalOnly,
    CriticalWithRoleAwareness
}
