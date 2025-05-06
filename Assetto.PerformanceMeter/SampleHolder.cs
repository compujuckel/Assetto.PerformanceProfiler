namespace Assetto.PerformanceMeter;

public class SampleHolder
{
    public List<double> CpuTimeMs { get; init; } = [];
    public List<double> GpuTimeMs { get; init; } = [];
    public List<double> DrawCalls { get; init; } = [];
    public List<double> SceneTriangles { get; init; } = [];
    public List<double> VramUsage { get; init; } = [];

    public void Record(in PerformanceMeterMappedFile meter)
    {
        CpuTimeMs.Add(Math.Round(meter.CpuTimeMs, 4));
        GpuTimeMs.Add(Math.Round(meter.GpuTimeMs, 4));
        DrawCalls.Add(meter.DrawCalls);
        SceneTriangles.Add(meter.SceneTriangles);
        VramUsage.Add(meter.VramUsage);
    }
}
