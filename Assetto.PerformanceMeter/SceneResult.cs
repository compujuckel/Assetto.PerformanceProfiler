namespace Assetto.PerformanceMeter;

public class SceneResult
{
    public SampleHolder Samples { get; }
    public SampleStatistics CpuTimeStatistics { get; }
    public SampleStatistics GpuTimeStatistics { get; }
    public SampleStatistics DrawCallsStatistics { get; }
    public SampleStatistics SceneTrianglesStatistics { get; }
    public SampleStatistics VramUsageStatistics { get; }

    public SceneResult(SampleHolder samples)
    {
        Samples = samples;
        CpuTimeStatistics = SampleStatistics.Calculate(samples.CpuTimeMs);
        GpuTimeStatistics = SampleStatistics.Calculate(samples.GpuTimeMs);
        DrawCallsStatistics = SampleStatistics.Calculate(samples.DrawCalls);
        SceneTrianglesStatistics = SampleStatistics.Calculate(samples.SceneTriangles);
        VramUsageStatistics = SampleStatistics.Calculate(samples.VramUsage);
    }
}
