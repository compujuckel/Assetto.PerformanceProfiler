using MathNet.Numerics.Statistics;

namespace Assetto.PerformanceProfiler.Model;

public class SampleStatistics
{
    public double P50 { get; private init; }
    public double P75 { get; private init; }
    public double P90 { get; private init; }
    public double P99 { get; private init; }
    public double Min { get; private init; }
    public double Max { get; private init; }
    public double Average { get; private init; }
    public double StdDev { get; private init; }
    
    private SampleStatistics() { }

    public static SampleStatistics Calculate(List<double> data)
    {
        return new SampleStatistics
        {
            P50 = Math.Round(data.Percentile(50), 2),
            P75 = Math.Round(data.Percentile(75), 2),
            P90 = Math.Round(data.Percentile(90), 2),
            P99 = Math.Round(data.Percentile(99), 2),
            Min = Math.Round(data.Min(), 2),
            Max = Math.Round(data.Max(), 2),
            Average = Math.Round(data.Average(), 2),
            StdDev = Math.Round(data.StandardDeviation(), 2)
        };
    }
}
