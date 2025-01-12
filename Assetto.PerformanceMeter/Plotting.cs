using ScottPlot;

namespace Assetto.PerformanceMeter;

public class Plotting
{
    public static void GenerateSignalPlot(IEnumerable<List<double>> samples, string title, string filename)
    {
        Plot myPlot = new();
        foreach (var list in samples)
        {
            myPlot.Add.Signal(list);
        }

        myPlot.Title(title);
        myPlot.Axes.Bottom.Min = 0;
        //myPlot.Axes.Bottom.Max = samples.Max(s => s.Count);
        myPlot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        myPlot.SavePng(filename, 800, 600);
    }
}
