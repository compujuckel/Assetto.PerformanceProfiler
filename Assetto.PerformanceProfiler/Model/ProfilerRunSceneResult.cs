namespace Assetto.PerformanceProfiler.Model;

public class ProfilerRunSceneResult(SampleHolder samples)
{
    public SampleHolder Samples { get; } = samples;
    public SampleStatistics CpuTimeStatistics { get; } = SampleStatistics.Calculate(samples.CpuTimeMs);
    public SampleStatistics GpuTimeStatistics { get; } = SampleStatistics.Calculate(samples.GpuTimeMs);
    public SampleStatistics DrawCallsStatistics { get; } = SampleStatistics.Calculate(samples.DrawCalls);
    public SampleStatistics SceneTrianglesStatistics { get; } = SampleStatistics.Calculate(samples.SceneTriangles);
    public SampleStatistics VramUsageStatistics { get; } = SampleStatistics.Calculate(samples.VramUsage);
}
