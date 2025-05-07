using Assetto.PerformanceMeter.Configuration;

namespace Assetto.PerformanceMeter;

public class Results
{
    public RunConfiguration Configuration { get; }
    public SystemInfo SystemInfo { get; }
    public Dictionary<string, SceneResult> Scenes { get; } = new();
    
    public Results(RunConfiguration configuration, List<SampleHolder> samples, SystemInfo systemInfo)
    {
        Configuration = configuration;
        SystemInfo = systemInfo;
        for (int i = 0; i < samples.Count; i++)
        {
            Scenes.Add(configuration.Scenes[i].Name, new SceneResult(samples[i]));
        }
    }
}
