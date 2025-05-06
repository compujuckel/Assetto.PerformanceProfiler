namespace Assetto.PerformanceMeter;

public class BatchResults
{
    public SystemInfo SystemInfo { get; }
    public Dictionary<string, BatchSceneResults> Scenes { get; set; } = new();
    
    public BatchResults(SystemInfo systemInfo)
    {
        SystemInfo = systemInfo;
    }

    public void AddResult(Results result)
    {
        foreach (var scene in result.Scenes)
        {
            if (!Scenes.TryGetValue(scene.Key, out var outResults))
            {
                outResults = new BatchSceneResults
                {
                    Configuration = result.Configuration.Scenes.First(s => s.Name == scene.Key), Results = []
                };
                Scenes.Add(scene.Key, outResults);
            }
            
            outResults.Results.Add(new BatchSceneResult
            {
                TrackName = result.Configuration.TrackName,
                TrackLayout = result.Configuration.TrackLayout,
                CarModel = result.Configuration.CarModel,
                CarSkin = result.Configuration.CarSkin,
                Result = scene.Value
            });
        }
    }
}
