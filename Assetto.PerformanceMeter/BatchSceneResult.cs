namespace Assetto.PerformanceMeter;

public class BatchSceneResult
{
    public required string TrackName { get; init; }
    public required string TrackLayout { get; init; } = "";
    public required string CarModel { get; init; }
    public required string CarSkin { get; init; } = "";
    public required SceneResult Result { get; init; }
}
