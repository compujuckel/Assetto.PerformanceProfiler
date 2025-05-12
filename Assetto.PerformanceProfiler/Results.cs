using Assetto.PerformanceProfiler.Configuration;

namespace Assetto.PerformanceProfiler;

public class Results
{
    public RunConfiguration Configuration { get; }
    public Dictionary<string, SceneResult> Scenes { get; } = new();
    
    public Results(RunConfiguration configuration, List<SampleHolder> samples)
    {
        Configuration = configuration;
        for (int i = 0; i < samples.Count; i++)
        {
            Scenes.Add(configuration.Scenes[i].Name, new SceneResult(samples[i]));
        }
    }
}
