using Microsoft.Data.Sqlite;
using PafiIntegrationSystemMonitor.Infrastructure;
using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class HistoryService
{
    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AppPaths.HistoryPath)!);

        await using var connection = new SqliteConnection($"Data Source={AppPaths.HistoryPath}");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS History(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  TimestampUtc TEXT NOT NULL,
  ItemName TEXT NOT NULL,
  ItemType TEXT NOT NULL,
  PreviousState TEXT NOT NULL,
  NewState TEXT NOT NULL,
  GroupName TEXT NOT NULL,
  Detail TEXT NOT NULL
);
DELETE FROM History WHERE TimestampUtc < datetime('now','-90 day');";
        await command.ExecuteNonQueryAsync();
    }

    public async Task AddAsync(HistoryEntry entry)
    {
        await using var connection = new SqliteConnection($"Data Source={AppPaths.HistoryPath}");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO History (TimestampUtc, ItemName, ItemType, PreviousState, NewState, GroupName, Detail)
VALUES ($timestamp, $itemName, $itemType, $prev, $new, $groupName, $detail);";
        command.Parameters.AddWithValue("$timestamp", entry.TimestampUtc.ToString("O"));
        command.Parameters.AddWithValue("$itemName", entry.ItemName);
        command.Parameters.AddWithValue("$itemType", entry.ItemType);
        command.Parameters.AddWithValue("$prev", entry.PreviousState);
        command.Parameters.AddWithValue("$new", entry.NewState);
        command.Parameters.AddWithValue("$groupName", entry.GroupName);
        command.Parameters.AddWithValue("$detail", entry.Detail);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<HistoryEntry>> QueryAsync(string? itemFilter, string? typeFilter, string? statusFilter, string? groupFilter, DateTime? fromUtc, DateTime? toUtc)
    {
        await using var connection = new SqliteConnection($"Data Source={AppPaths.HistoryPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, TimestampUtc, ItemName, ItemType, PreviousState, NewState, GroupName, Detail
FROM History
WHERE ($itemFilter IS NULL OR ItemName LIKE $itemLike)
  AND ($typeFilter IS NULL OR ItemType = $typeFilter)
  AND ($statusFilter IS NULL OR NewState = $statusFilter)
  AND ($groupFilter IS NULL OR GroupName = $groupFilter)
  AND ($fromUtc IS NULL OR TimestampUtc >= $fromUtc)
  AND ($toUtc IS NULL OR TimestampUtc <= $toUtc)
ORDER BY TimestampUtc DESC;";

        command.Parameters.AddWithValue("$itemFilter", (object?)itemFilter ?? DBNull.Value);
        command.Parameters.AddWithValue("$itemLike", $"%{itemFilter ?? string.Empty}%");
        command.Parameters.AddWithValue("$typeFilter", (object?)typeFilter ?? DBNull.Value);
        command.Parameters.AddWithValue("$statusFilter", (object?)statusFilter ?? DBNull.Value);
        command.Parameters.AddWithValue("$groupFilter", (object?)groupFilter ?? DBNull.Value);
        command.Parameters.AddWithValue("$fromUtc", fromUtc.HasValue ? fromUtc.Value.ToString("O") : DBNull.Value);
        command.Parameters.AddWithValue("$toUtc", toUtc.HasValue ? toUtc.Value.ToString("O") : DBNull.Value);

        var records = new List<HistoryEntry>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            records.Add(new HistoryEntry
            {
                Id = reader.GetInt64(0),
                TimestampUtc = DateTime.Parse(reader.GetString(1)).ToUniversalTime(),
                ItemName = reader.GetString(2),
                ItemType = reader.GetString(3),
                PreviousState = reader.GetString(4),
                NewState = reader.GetString(5),
                GroupName = reader.GetString(6),
                Detail = reader.GetString(7),
            });
        }

        return records;
    }
}
