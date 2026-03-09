using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using PafiIntegrationSystemMonitor.Commands;
using PafiIntegrationSystemMonitor.Models;
using PafiIntegrationSystemMonitor.Services;

namespace PafiIntegrationSystemMonitor.ViewModels;

public sealed class HistoryViewModel : ViewModelBase
{
    private readonly HistoryService _historyService;

    public HistoryViewModel(HistoryService historyService)
    {
        _historyService = historyService;
        Records = new ObservableCollection<HistoryEntry>();
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync, () => Records.Any());

        DateFromUtc = DateTime.UtcNow.AddDays(-7);
        DateToUtc = DateTime.UtcNow;
    }

    public ObservableCollection<HistoryEntry> Records { get; }

    public string? ItemFilter { get; set; }
    public string? TypeFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? GroupFilter { get; set; }
    public DateTime? DateFromUtc { get; set; }
    public DateTime? DateToUtc { get; set; }

    public ICommand RefreshCommand { get; }
    public ICommand ExportCsvCommand { get; }

    public async Task RefreshAsync()
    {
        var results = await _historyService.QueryAsync(ItemFilter, TypeFilter, StatusFilter, GroupFilter, DateFromUtc, DateToUtc);
        Records.Clear();
        foreach (var result in results)
        {
            Records.Add(result);
        }
    }

    private async Task ExportCsvAsync()
    {
        var exportFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PafiMonitorExports");
        Directory.CreateDirectory(exportFolder);
        var path = Path.Combine(exportFolder, $"history-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

        var sb = new StringBuilder();
        sb.AppendLine("TimestampUtc,ItemName,ItemType,PreviousState,NewState,GroupName,Detail");
        foreach (var item in Records)
        {
            sb.AppendLine($"{item.TimestampUtc:O},\"{Escape(item.ItemName)}\",{item.ItemType},{item.PreviousState},{item.NewState},\"{Escape(item.GroupName)}\",\"{Escape(item.Detail)}\"");
        }

        await File.WriteAllTextAsync(path, sb.ToString());
    }

    private static string Escape(string value) => value.Replace("\"", "\"\"");
}
