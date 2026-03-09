namespace PafiIntegrationSystemMonitor.Models;

public sealed class IntegrationGroupDefinition
{
    public string Name { get; set; } = string.Empty;
    public GroupHealthRuleType HealthRule { get; set; } = GroupHealthRuleType.CriticalWithRoleAwareness;
}
