using System.Text;
using ClosedXML.Excel;
using Serilog;

namespace Assetto.PerformanceProfiler;

public class ExcelReportGenerator
{
    private static readonly List<ExcelSheet> Sheets =
    [
        new("CPU", r => r.CpuTimeStatistics, GenerateNumberFormat(2, "ms")),
        new("GPU", r => r.GpuTimeStatistics, GenerateNumberFormat(2, "ms")),
        new("Draw Calls", r => r.DrawCallsStatistics, GenerateNumberFormat(0)),
        new("Scene Triangles", r => r.SceneTrianglesStatistics, GenerateNumberFormat(0)),
        new("VRAM Usage", r => r.VramUsageStatistics, GenerateNumberFormat(0, "MB"))
    ];
    
    public void GenerateBatch(BatchResults results, string path)
    {
        ReadOnlySpan<int> valueColumns = [2, 4, 6, 8, 10, 12, 14, 16];
        ReadOnlySpan<int> percentageColumns = [3, 5, 7, 9, 11, 13, 15, 17];

        try
        {
            using var wb = new XLWorkbook();

            foreach (var sheet in Sheets)
            {
                var ws = wb.Worksheets.Add(sheet.Name);

                ws.ColumnWidth = 12;

                var target = ws.FirstCell();
                foreach (var scene in results.Scenes)
                {
                    target.Value = scene.Key;
                    target.Style.Font.Bold = true;
                    target.Style.Font.FontSize = 18;
                    target = target.CellBelow();
                    var table = target.InsertTable(GeneratePerformanceRows(scene.Value.Results, sheet.Accessor), true);
                    table.Sort("Avg ASC");
                    target = table.LastRow().RowBelow(2).FirstCell();
                }
                
                // Title
                ws.Column(1).Width = 50;

                // Values
                foreach (var col in valueColumns)
                {
                    ws.Column(col).Style.NumberFormat.Format = sheet.NumberFormat;
                }

                // Percentages
                foreach (var col in percentageColumns)
                {
                    var style = ws.Column(col).Style;
                    style.NumberFormat.Format = "[Red]0.0%;-0.0%;0.0%";
                    style.Font.Italic = true;
                }
            }

            wb.SaveAs(path);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to generate excel report");
        }
    }

    private static string GenerateNumberFormat(int precision, string? unit = "")
    {
        var sb = new StringBuilder();
        sb.Append("#,##0");

        if (precision > 0)
        {
            sb.Append('.');
            for (int i = 0; i < precision; i++)
            {
                sb.Append('0');
            }
        }

        if (unit != null)
        {
            sb.Append("\" ");
            sb.Append(unit);
            sb.Append('"');
        }

        return sb.ToString();
    }

    private static List<ExcelPerformanceRow> GeneratePerformanceRows(List<BatchSceneResult> results,
        Func<SceneResult, SampleStatistics> accessor)
    {
        var singleTrack = results.DistinctBy(r => $"{r.TrackName}-{r.TrackLayout}").Count() == 1;
        var ret = new List<ExcelPerformanceRow>();
        var baseline = accessor(results.First().Result);
        
        foreach (var result in results)
        {
            var name = GenerateRowName(result, singleTrack);
            var stats = accessor(result.Result);
            
            var row = new ExcelPerformanceRow(name, 
                stats.Average, GetIncrease(baseline.Average, stats.Average),
                stats.Min,  GetIncrease(baseline.Min, stats.Min),
                stats.Max,  GetIncrease(baseline.Max, stats.Max),
                stats.StdDev, GetIncrease(baseline.StdDev, stats.StdDev),
                stats.P50,  GetIncrease(baseline.P50, stats.P50),
                stats.P75, GetIncrease(baseline.P75, stats.P75),
                stats.P90, GetIncrease(baseline.P90, stats.P90),
                stats.P99, GetIncrease(baseline.P99, stats.P99));
            ret.Add(row);
        }
        
        return ret;
    }

    private static double GetIncrease(double baseline, double value)
    {
        var ret = value / baseline - 1;
        if (double.IsNaN(ret)) ret = 0;
        if (double.IsInfinity(ret)) ret = 99.999;
        if (double.IsNegativeInfinity(ret)) ret = -99.999;

        return ret;
    }

    private static string GenerateRowName(BatchSceneResult result, bool singleTrack)
    {
        var sb = new StringBuilder();
            
        if (!singleTrack)
        {
            sb.Append(result.TrackName);

            if (!string.IsNullOrEmpty(result.TrackLayout))
            {
                sb.Append(" (");
                sb.Append(result.TrackLayout);
                sb.Append(')');
            }
            
            sb.Append(" - ");
        }
            
        sb.Append(result.CarModel);
        if (!string.IsNullOrEmpty(result.CarSkin))
        {
            sb.Append(" (");
            sb.Append(result.CarSkin);
            sb.Append(')');
        }
            
        return sb.ToString();
    }
}

internal record ExcelSheet(string Name, Func<SceneResult, SampleStatistics> Accessor, string NumberFormat);
internal record ExcelPerformanceRow(string Name, double Avg, double AvgP, double Min, double MinP, double Max, double MaxP, double StdDev, double StdDevP, double P50, double P50P, double P75, double P75P, double P90, double P90P, double P99, double P99P);