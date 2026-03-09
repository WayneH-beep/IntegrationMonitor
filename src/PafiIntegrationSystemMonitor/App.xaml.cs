using System.Windows;
using PafiIntegrationSystemMonitor.Infrastructure;
using Serilog;

namespace PafiIntegrationSystemMonitor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AppPaths.LogPath)!);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(AppPaths.LogPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
