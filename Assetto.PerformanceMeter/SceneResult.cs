namespace Assetto.PerformanceMeter;

public class SceneResult
{
    public SampleHolder Samples { get; init; }
    public SampleStatistics CpuTimeStatistics { get; init; }
    public SampleStatistics GpuTimeStatistics { get; init; }
    public SampleStatistics DrawCallsStatistics { get; init; }
    public SampleStatistics SceneTrianglesStatistics { get; init; }
    public SampleStatistics VramUsageStatistics { get; init; }
    
    public SceneResult() {}

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
