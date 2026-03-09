using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class GroupHealthService
{
    public IReadOnlyList<GroupRuntimeState> Compute(AppConfiguration config, IReadOnlyDictionary<Guid, ItemRuntimeState> states)
    {
        var groups = config.Items.Where(i => i.IsEnabled).GroupBy(i => i.GroupName);
        var list = new List<GroupRuntimeState>();

        foreach (var group in groups)
        {
            var items = group.ToList();
            if (items.Any(x => !states.ContainsKey(x.Id)))
            {
                list.Add(new GroupRuntimeState { GroupName = group.Key, Status = MonitorStatus.Unknown, Detail = "Awaiting initial check" });
                continue;
            }

            var groupRule = config.Groups.FirstOrDefault(g => g.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.HealthRule
                            ?? GroupHealthRuleType.CriticalWithRoleAwareness;

            var criticalFailed = items.Where(i => i.IsCritical).Where(i => states[i.Id].Status == MonitorStatus.Failed).ToList();
            var anyWarning = items.Any(i => states[i.Id].Status == MonitorStatus.Warning);
            var anyNonCriticalFailure = items.Where(i => !i.IsCritical).Any(i => states[i.Id].Status == MonitorStatus.Failed);

            if (criticalFailed.Count == 0 && !anyWarning && !anyNonCriticalFailure)
            {
                list.Add(new GroupRuntimeState { GroupName = group.Key, Status = MonitorStatus.Healthy, Detail = "All monitored items healthy" });
                continue;
            }

            if (criticalFailed.Count > 0)
            {
                if (groupRule == GroupHealthRuleType.CriticalWithRoleAwareness && IsDegradedPairOnly(items, states, criticalFailed))
                {
                    list.Add(new GroupRuntimeState { GroupName = group.Key, Status = MonitorStatus.Warning, Detail = "Primary/secondary degraded state" });
                }
                else
                {
                    list.Add(new GroupRuntimeState { GroupName = group.Key, Status = MonitorStatus.Failed, Detail = "Critical failure detected" });
                }
                continue;
            }

            list.Add(new GroupRuntimeState { GroupName = group.Key, Status = MonitorStatus.Warning, Detail = "Warning or non-critical failure present" });
        }

        return list;
    }

    private static bool IsDegradedPairOnly(List<MonitoredItemDefinition> items, IReadOnlyDictionary<Guid, ItemRuntimeState> states, List<MonitoredItemDefinition> criticalFailed)
    {
        foreach (var failed in criticalFailed)
        {
            if (failed.Role != EndpointRole.Primary || string.IsNullOrWhiteSpace(failed.PairKey))
            {
                return false;
            }

            var secondary = items.FirstOrDefault(i => i.PairKey == failed.PairKey && i.Role == EndpointRole.Secondary);
            if (secondary is null || states[secondary.Id].Status != MonitorStatus.Healthy)
            {
                return false;
            }
        }

        return true;
    }
}
