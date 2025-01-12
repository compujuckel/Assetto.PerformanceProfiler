namespace Assetto.PerformanceMeter;

public class SampleHolder
{
    public List<double> CpuTimeMs { get; } = [];
    public List<double> GpuTimeMs { get; } = [];
    public List<double> DrawCalls { get; } = [];
    public List<double> SceneTriangles { get; } = [];
    public List<double> VramUsage { get; } = [];

    public void Record(in PerformanceMeterMappedFile meter)
    {
        CpuTimeMs.Add(Math.Round(meter.CpuTimeMs, 4));
        GpuTimeMs.Add(Math.Round(meter.GpuTimeMs, 4));
        DrawCalls.Add(meter.DrawCalls);
        SceneTriangles.Add(meter.SceneTriangles);
        VramUsage.Add(meter.VramUsage);
    }
}
