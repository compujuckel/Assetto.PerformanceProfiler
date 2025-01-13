using System.Diagnostics;

namespace Assetto.PerformanceMeter;

public class ACLauncher
{
    private void WriteRaceCfg(string track, string layout, string car, string skin)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "cfg");
        File.WriteAllText(Path.Join(path, "race_perfmeter.ini"), ACLauncherConfig.GetRaceCfg(track, layout, car, skin));
    }

    public async Task RunAndWaitAsync(string track, string layout, string car, string skin, CancellationToken token = default)
    {
        const string acRoot = @"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa";
        WriteRaceCfg(track, layout, car, skin);

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = Path.Join(acRoot, "acs.exe"),
            UseShellExecute = false,
            WorkingDirectory = acRoot,
            EnvironmentVariables =
            {
                //["AC_CFG_PROGRAM_NAME"] = Key,
                ["AC_CFG_RACE_INI"] = "race_perfmeter.ini"
            }
        })!;

        await using var reg = token.Register(() => process.CloseMainWindow());
        await process.WaitForExitAsync(CancellationToken.None);
    }
}
