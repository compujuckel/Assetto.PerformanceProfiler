using System.Diagnostics;
using System.Text.Json;
using Assetto.PerformanceMeter.Configuration;

namespace Assetto.PerformanceMeter;

public class ACLauncher
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        IncludeFields = true
    };
    
    public string GetRootDirectory()
    {
        return @"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa";
    }

    public void WriteSceneConfiguration(List<SceneConfiguration> scenes)
    {
        var path = Path.Join(GetRootDirectory(), "apps/lua/PerformanceMeter/scenes.json");

        using var file = File.Create(path);
        JsonSerializer.Serialize(file, scenes, JsonSerializerOptions);
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
            FileName = Path.Join(GetRootDirectory(), "acs.exe"),
            UseShellExecute = false,
            WorkingDirectory = GetRootDirectory(),
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
