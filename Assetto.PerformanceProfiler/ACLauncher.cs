using System.Diagnostics;
using System.Text.Json;
using Assetto.PerformanceProfiler.Configuration;
using Microsoft.Win32;

namespace Assetto.PerformanceProfiler;

public class ACLauncher
{
    public string RootDirectory { get; }
    public string AppDirectory { get; }
    public string ContentCarsDirectory { get; }
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        // Required for Vector3
        IncludeFields = true
    };
    
    public ACLauncher()
    {
        RootDirectory = GetRootDirectory() ?? throw new InvalidOperationException("Could not find Assetto Corsa root directory");;
        AppDirectory = Path.Join(RootDirectory, "apps", "lua", "ACPerformanceProfiler");
        ContentCarsDirectory = Path.Join(RootDirectory, "content", "cars");
    }
    
    private static string? GetRootDirectory()
    {
        var key = Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244210", "InstallLocation", null);
        return key?.ToString();
    }

    public void WriteAppConfiguration(AppConfiguration configuration)
    {
        // TODO Detect when app is not present
        var path = Path.Join(AppDirectory, "scenes.json");

        using var file = File.Create(path);
        JsonSerializer.Serialize(file, configuration, JsonSerializerOptions);
    }
    
    private void WriteRaceCfg(string track, string layout, string car, string skin)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "cfg");
        File.WriteAllText(Path.Join(path, "race_perfmeter.ini"), ACLauncherConfig.GetRaceCfg(track, layout, car, skin));
    }

    public async Task RunAndWaitAsync(string track, string layout, string car, string skin, CancellationToken token = default)
    {
        WriteRaceCfg(track, layout, car, skin);

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = Path.Join(RootDirectory, "acs.exe"),
            UseShellExecute = false,
            WorkingDirectory = RootDirectory,
            EnvironmentVariables =
            {
                //["AC_CFG_PROGRAM_NAME"] = Key,
                ["AC_CFG_RACE_INI"] = "race_perfmeter.ini"
            }
        })!;

        WindowHelper.BringProcessToFront(process);

        await using var reg = token.Register(() => process.CloseMainWindow());
        await process.WaitForExitAsync(CancellationToken.None);
    }
}
