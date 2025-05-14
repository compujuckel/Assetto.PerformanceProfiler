namespace Assetto.PerformanceProfiler.Model;

public class SceneResult
{
    public required string TrackName { get; init; }
    public required string TrackLayout { get; init; } = "";
    public required string CarModel { get; init; }
    public required string CarSkin { get; init; } = "";
    public required ProfilerRunSceneResult Result { get; init; }
}
