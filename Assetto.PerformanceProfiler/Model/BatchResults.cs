using System.Text.Json;

namespace Assetto.PerformanceProfiler.Model;

public class BatchResults
{
    public SystemInfo SystemInfo { get; }
    public Dictionary<string, BatchSceneResult> Scenes { get; init; } = new();
    
    public BatchResults(SystemInfo systemInfo)
    {
        SystemInfo = systemInfo;
    }

    public void ToFile(string path)
    {
        using var file = File.Create(path);
        JsonSerializer.Serialize(file, this);
    }

    public static BatchResults FromFile(string path)
    {
        using var file = File.OpenRead(path);
        return JsonSerializer.Deserialize<BatchResults>(file)!;
    }

    public void AddRunResult(ProfilerRunResult profilerRunResult)
    {
        foreach (var sceneConfiguration in profilerRunResult.Configuration.Scenes)
        {
            if (!Scenes.TryGetValue(sceneConfiguration.Name, out var outResults))
            {
                outResults = new BatchSceneResult
                {
                    Configuration = sceneConfiguration
                };
                Scenes.Add(sceneConfiguration.Name, outResults);
            }
            
            outResults.Results.Add(new SceneResult {
                TrackName = profilerRunResult.Configuration.TrackName,
                TrackLayout = profilerRunResult.Configuration.TrackLayout,
                CarModel = profilerRunResult.Configuration.CarModel,
                CarSkin = profilerRunResult.Configuration.CarSkin,
                Result = profilerRunResult.Results[sceneConfiguration.Name]
            });
        }
    }
}
