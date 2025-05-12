namespace Assetto.PerformanceProfiler.Configuration;

public class RunConfiguration
{
    public required string TrackName { get; init; }
    public required string TrackLayout { get; init; } = "";
    public required string CarModel { get; init; }
    public required string CarSkin { get; init; } = "";
    public string? CarRenameTo { get; init; }
    public required List<SceneConfiguration> Scenes { get; init; }
}
