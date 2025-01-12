namespace Assetto.PerformanceMeter;

public class Results
{
    public Dictionary<string, SceneResult> Scenes { get; } = new();
    
    public Results(List<SampleHolder> samples)
    {
        for (int i = 0; i < samples.Count; i++)
        {
            Scenes.Add(i.ToString(), new SceneResult(samples[i]));
        }
    }
}
