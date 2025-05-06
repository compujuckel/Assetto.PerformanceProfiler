namespace Assetto.PerformanceMeter;

public class BatchSceneResults
{
    public required SceneConfiguration Configuration { get; init; }
    public required List<BatchSceneResult> Results { get; init; }
}
