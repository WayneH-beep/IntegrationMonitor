using PafiIntegrationSystemMonitor.Models;
using Serilog;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class MonitoringEngine : IAsyncDisposable
{
    private readonly MonitorProbeService _probeService;
    private readonly HistoryService _historyService;
    private readonly Dictionary<Guid, ItemRuntimeState> _states = new();
    private CancellationTokenSource? _cts;

    public MonitoringEngine(MonitorProbeService probeService, HistoryService historyService)
    {
        _probeService = probeService;
        _historyService = historyService;
    }

    public event Action<MonitoredItemDefinition, ItemRuntimeState>? ItemUpdated;
    public IReadOnlyDictionary<Guid, ItemRuntimeState> States => _states;

    public void Start(AppConfiguration config)
    {
        Stop();
        _cts = new CancellationTokenSource();
        foreach (var item in config.Items.Where(i => i.IsEnabled))
        {
            _ = PollItemLoopAsync(item, config, _cts.Token);
        }
    }

    public void Stop()
    {
        if (_cts is null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    public async Task CheckNowAsync(MonitoredItemDefinition item, AppConfiguration config)
    {
        var timeoutMs = item.TimeoutMilliseconds > 0 ? item.TimeoutMilliseconds : config.DefaultTimeoutMilliseconds;
        var checkTime = DateTime.UtcNow;
        (MonitorStatus status, string detail, long? responseMs) result = item.Type switch
        {
            MonitoredItemType.Service => await _probeService.CheckServiceAsync(item.Target),
            MonitoredItemType.Device => await _probeService.CheckDeviceAsync(item.Target, timeoutMs),
            _ => await _probeService.CheckTcpEndpointAsync(item.Target, item.Port, timeoutMs),
        };

        if (!_states.TryGetValue(item.Id, out var state))
        {
            state = new ItemRuntimeState { ItemId = item.Id, LastChangedUtc = checkTime };
            _states[item.Id] = state;
        }

        var previous = state.Status;
        state.Status = result.status;
        state.Detail = result.responseMs.HasValue ? $"{result.detail} ({result.responseMs} ms)" : result.detail;
        state.ResponseTimeMs = result.responseMs;
        state.LastCheckedUtc = checkTime;

        if (previous != state.Status)
        {
            state.LastChangedUtc = checkTime;
            await _historyService.AddAsync(new HistoryEntry
            {
                TimestampUtc = checkTime,
                ItemName = item.FriendlyName,
                ItemType = item.Type.ToString(),
                PreviousState = previous.ToString(),
                NewState = state.Status.ToString(),
                GroupName = item.GroupName,
                Detail = state.Detail,
            });
        }

        ItemUpdated?.Invoke(item, state);
    }

    private async Task PollItemLoopAsync(MonitoredItemDefinition item, AppConfiguration config, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CheckNowAsync(item, config);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Monitor check failed for {Item}", item.FriendlyName);
            }

            var interval = item.PollIntervalSeconds > 0 ? item.PollIntervalSeconds : config.DefaultPollIntervalSeconds;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        Stop();
        return ValueTask.CompletedTask;
    }
}
