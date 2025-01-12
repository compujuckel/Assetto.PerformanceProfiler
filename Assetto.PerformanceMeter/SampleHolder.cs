namespace Assetto.PerformanceMeter;

public class SampleHolder
{
    public List<double> CpuTimeMs { get; } = [];
    public List<double> GpuTimeMs { get; } = [];
    public List<double> DrawCalls { get; } = [];
    public List<double> SceneTriangles { get; } = [];
    public List<double> VramUsage { get; } = [];
    public List<int> SceneIndices { get; } = [];

    public void Record(in PerformanceMeterMappedFile meter)
    {
        if (SceneIndices.Count == 0 || SceneIndices[^1] < meter.Scene)
        {
            SceneIndices.Add(CpuTimeMs.Count);
        }
        
        CpuTimeMs.Add(meter.CpuTimeMs);
        GpuTimeMs.Add(meter.GpuTimeMs);
        DrawCalls.Add(meter.DrawCalls);
        SceneTriangles.Add(meter.SceneTriangles);
        VramUsage.Add(meter.VramUsage);
    }
}
