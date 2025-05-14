using Assetto.PerformanceProfiler.Configuration;

namespace Assetto.PerformanceProfiler.Model;

public class BatchSceneResult
{
    public required SceneConfiguration Configuration { get; init; }
    public List<SceneResult> Results { get; init; } = [];
}
