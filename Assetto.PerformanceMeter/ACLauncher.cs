using System.Diagnostics;

namespace Assetto.PerformanceMeter;

public class ACLauncher
{
    private void WriteRaceCfg()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assetto Corsa", "cfg");
        File.WriteAllText(Path.Join(path, "race_perfmeter.ini"), ACLauncherConfigs.GetRaceCfg("endlessfloor", "", "amy_honda_ek9_turbo", ""));
    }

    public async Task RunAndWaitAsync(CancellationToken token = default)
    {
        var acRoot = @"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa";
        WriteRaceCfg();

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
