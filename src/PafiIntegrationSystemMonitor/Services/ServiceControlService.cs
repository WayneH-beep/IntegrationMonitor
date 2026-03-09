using System.ServiceProcess;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class ServiceControlService
{
    public bool CanControlService(string serviceName)
    {
        try
        {
            using var service = new ServiceController(serviceName);
            return service.CanStop;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool success, string detail)> RestartAsync(string serviceName)
    {
        try
        {
            using var service = new ServiceController(serviceName);
            if (service.Status != ServiceControllerStatus.Stopped && service.CanStop)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            }

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            return (true, "Service restarted successfully");
        }
        catch (Exception ex)
        {
            await Task.CompletedTask;
            return (false, ex.Message);
        }
    }
}
