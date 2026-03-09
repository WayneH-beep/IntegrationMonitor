using System.ServiceProcess;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class ServiceDiscoveryService
{
    public IReadOnlyList<string> GetAllServiceNames(string? search = null)
    {
        try
        {
            var services = ServiceController.GetServices().Select(s => s.ServiceName);
            if (!string.IsNullOrWhiteSpace(search))
            {
                services = services.Where(s => s.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            return services.OrderBy(s => s).ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
