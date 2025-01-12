using MathNet.Numerics.Statistics;

namespace Assetto.PerformanceMeter;

public class PerfStatistics
{
    public double P50 { get; init; }
    public double P75 { get; init; }
    public double P90 { get; init; }
    public double P99 { get; init; }
    public double Average { get; init; }
    public double StdDev { get; init; }

    public static PerfStatistics Calculate(List<double> data)
    {
        return new PerfStatistics()
        {
            P50 = data.Percentile(50),
            P75 = data.Percentile(75),
            P90 = data.Percentile(90),
            P99 = data.Percentile(99),
            Average = data.Average(),
            StdDev = data.StandardDeviation()
        };
    }
}
