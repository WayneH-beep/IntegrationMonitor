# Pafi Integration System Monitor

Pafi Integration System Monitor is a reusable Windows 10/11 monitoring platform for engineers. It monitors:
- Local Windows services
- Network devices by IP/hostname (ping)
- TCP endpoints (host + port)
- User-defined integration groups with configurable health rules

The included Lenel/KONE setup is only a **sample config**. Runtime behavior is generic and user-configurable.

## Technology
- .NET 8
- C#
- WPF (MVVM)
- SQLite (history)
- JSON (configuration)
- WiX Toolset v4 MSI installer

## Solution structure
- `src/PafiIntegrationSystemMonitor` - WPF app project
- `installer/PafiIntegrationSystemMonitor.Installer` - WiX v4 MSI project
- `src/PafiIntegrationSystemMonitor/Data/sample-config.json` - default first-run sample

## Runtime data location
Writable runtime files are not stored in Program Files.
- Config: `%LOCALAPPDATA%\Pafi Integration System Monitor\config.json`
- History: `%LOCALAPPDATA%\Pafi Integration System Monitor\history.db`
- Logs: `%LOCALAPPDATA%\Pafi Integration System Monitor\logs\`

On first run, if `config.json` does not exist, the app copies `Data/sample-config.json` to LocalAppData.

## Build app in Visual Studio
1. Open `PafiIntegrationSystemMonitor.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. Build solution.
4. Run `PafiIntegrationSystemMonitor` project.

## Build app from CLI
```powershell
dotnet restore

dotnet build .\src\PafiIntegrationSystemMonitor\PafiIntegrationSystemMonitor.csproj -c Release
```

## Publish self-contained app payload for MSI
```powershell
dotnet publish .\src\PafiIntegrationSystemMonitor\PafiIntegrationSystemMonitor.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=false
```

This creates publish output at:
`src\PafiIntegrationSystemMonitor\bin\Release\net8.0-windows\win-x64\publish\`

## Build MSI (WiX Toolset v4)
After publish output exists:
```powershell
dotnet build .\installer\PafiIntegrationSystemMonitor.Installer\PafiIntegrationSystemMonitor.Installer.wixproj -c Release
```

MSI output:
`installer\PafiIntegrationSystemMonitor.Installer\bin\x64\Release\`

## Installer behavior
- Installs to `Program Files\Pafi Integration System Monitor`
- Adds Start Menu shortcut
- Optional Desktop shortcut controlled by MSI property: `INSTALLDESKTOPSHORTCUT=1`
- Supports uninstall in Windows Apps/Programs
- Supports major upgrades through stable `UpgradeCode`


## WiX harvesting troubleshooting
If installer build reports many `HEAT5151` warnings/errors while harvesting the publish folder:
1. Ensure the app publish step ran first for the same configuration/runtime (`Release`, `win-x64`).
2. Build the installer with the same configuration:
   ```powershell
   dotnet build .\installer\PafiIntegrationSystemMonitor.Installer\PafiIntegrationSystemMonitor.Installer.wixproj -c Release
   ```
3. The installer project is configured to harvest files only and suppress COM/registry harvesting, which avoids reflection-related failures on runtime files from self-contained .NET publish output.

## Notes
- Service restart requires appropriate Windows permissions.
- History retention automatically purges records older than 90 days.
