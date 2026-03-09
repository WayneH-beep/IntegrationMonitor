using System;
using System.IO;
using System.Text.Json;
using PafiIntegrationSystemMonitor.Infrastructure;
using PafiIntegrationSystemMonitor.Models;

namespace PafiIntegrationSystemMonitor.Services;

public sealed class ConfigurationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public async Task<AppConfiguration> LoadAsync()
    {
        Directory.CreateDirectory(AppPaths.AppDataFolder);
        if (!File.Exists(AppPaths.ConfigPath))
        {
            await InitializeFromSampleAsync();
        }

        await using var stream = File.OpenRead(AppPaths.ConfigPath);
        var config = await JsonSerializer.DeserializeAsync<AppConfiguration>(stream, JsonOptions);
        return config ?? new AppConfiguration();
    }

    public async Task SaveAsync(AppConfiguration config)
    {
        Directory.CreateDirectory(AppPaths.AppDataFolder);
        await using var stream = File.Create(AppPaths.ConfigPath);
        await JsonSerializer.SerializeAsync(stream, config, JsonOptions);
    }

    private static async Task InitializeFromSampleAsync()
    {
        Directory.CreateDirectory(AppPaths.AppDataFolder);
        var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "sample-config.json");
        if (File.Exists(samplePath))
        {
            await using var source = File.OpenRead(samplePath);
            await using var target = File.Create(AppPaths.ConfigPath);
            await source.CopyToAsync(target);
            return;
        }

        var defaultConfig = new AppConfiguration();
        await using var stream = File.Create(AppPaths.ConfigPath);
        await JsonSerializer.SerializeAsync(stream, defaultConfig, JsonOptions);
    }
}
