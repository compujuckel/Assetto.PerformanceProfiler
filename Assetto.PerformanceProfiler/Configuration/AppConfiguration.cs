namespace Assetto.PerformanceProfiler.Configuration;

public class AppConfiguration
{
    public int CurrentRun { get; init; }
    public int TotalRuns { get; init; }
    public required List<SceneConfiguration> Scenes { get; init; }
}
