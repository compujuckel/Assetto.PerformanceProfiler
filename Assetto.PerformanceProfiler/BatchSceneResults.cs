using Assetto.PerformanceProfiler.Configuration;

namespace Assetto.PerformanceProfiler;

public class BatchSceneResults
{
    public required SceneConfiguration Configuration { get; init; }
    public required List<BatchSceneResult> Results { get; init; }
}
