namespace Assetto.PerformanceProfiler;

public class SampleHolder
{
    public List<double> CpuTimeMs { get; init; } = [];
    public List<double> GpuTimeMs { get; init; } = [];
    public List<double> DrawCalls { get; init; } = [];
    public List<double> SceneTriangles { get; init; } = [];
    public List<double> VramUsage { get; init; } = [];

    public void Record(in PerformanceProfilerMappedFile profiler)
    {
        CpuTimeMs.Add(Math.Round(profiler.CpuTimeMs, 4));
        GpuTimeMs.Add(Math.Round(profiler.GpuTimeMs, 4));
        DrawCalls.Add(profiler.DrawCalls);
        SceneTriangles.Add(profiler.SceneTriangles);
        VramUsage.Add(profiler.VramUsage);
    }
}
