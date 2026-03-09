using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class MonitorProbeService
{
    public Task<(MonitorStatus status, string detail, long? responseTime)> CheckServiceAsync(string serviceName)
    {
        try
        {
            using var controller = new ServiceController(serviceName);
            var status = controller.Status switch
            {
                ServiceControllerStatus.Running => MonitorStatus.Healthy,
                ServiceControllerStatus.StartPending or ServiceControllerStatus.StopPending or ServiceControllerStatus.Paused => MonitorStatus.Warning,
                _ => MonitorStatus.Failed,
            };
            return Task.FromResult((status, controller.Status.ToString(), (long?)null));
        }
        catch (Exception ex)
        {
            return Task.FromResult((MonitorStatus.Failed, ex.Message, (long?)null));
        }
    }

    public async Task<(MonitorStatus status, string detail, long? responseTime)> CheckDeviceAsync(string host, int timeoutMs)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, timeoutMs);
            return reply.Status switch
            {
                IPStatus.Success => (MonitorStatus.Healthy, "Reachable", reply.RoundtripTime),
                IPStatus.TimedOut => (MonitorStatus.Failed, "Ping timeout", null),
                IPStatus.DestinationHostUnreachable => (MonitorStatus.Failed, "Host unreachable", null),
                _ => (MonitorStatus.Failed, reply.Status.ToString(), null),
            };
        }
        catch (Exception ex)
        {
            return (MonitorStatus.Failed, ex.Message, null);
        }
    }

    public async Task<(MonitorStatus status, string detail, long? responseTime)> CheckTcpEndpointAsync(string host, int port, int timeoutMs)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(timeoutMs);
            await client.ConnectAsync(host, port, cts.Token);
            stopwatch.Stop();
            return (MonitorStatus.Healthy, "TCP connected", stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            return (MonitorStatus.Failed, "TCP timeout", null);
        }
        catch (Exception ex)
        {
            return (MonitorStatus.Failed, ex.Message, null);
        }
    }
}
