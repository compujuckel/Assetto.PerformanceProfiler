using Assetto.PerformanceProfiler.Configuration;

namespace Assetto.PerformanceProfiler.Model;

public class ProfilerRunResult
{
    public RunConfiguration Configuration { get; }
    public Dictionary<string, ProfilerRunSceneResult> Results { get; } = new();
    
    public ProfilerRunResult(RunConfiguration configuration, List<SampleHolder> samples)
    {
        Configuration = configuration;
        for (int i = 0; i < samples.Count; i++)
        {
            Results.Add(configuration.Scenes[i].Name, new ProfilerRunSceneResult(samples[i]));
        }
    }
}
