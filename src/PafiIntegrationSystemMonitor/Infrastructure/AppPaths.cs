namespace PafiIntegrationSystemMonitor.Infrastructure;

public static class AppPaths
{
    public const string CompanyFolder = "Pafi Integration System Monitor";

    public static string AppDataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        CompanyFolder);

    public static string ConfigPath => Path.Combine(AppDataFolder, "config.json");
    public static string HistoryPath => Path.Combine(AppDataFolder, "history.db");
    public static string LogPath => Path.Combine(AppDataFolder, "logs", "monitor-.log");
}
