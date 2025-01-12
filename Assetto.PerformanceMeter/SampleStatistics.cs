using MathNet.Numerics.Statistics;

namespace Assetto.PerformanceMeter;

public class SampleStatistics
{
    public double P50 { get; init; }
    public double P75 { get; init; }
    public double P90 { get; init; }
    public double P99 { get; init; }
    public double Min { get; init; }
    public double Max { get; init; }
    public double Average { get; init; }
    public double StdDev { get; init; }

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
