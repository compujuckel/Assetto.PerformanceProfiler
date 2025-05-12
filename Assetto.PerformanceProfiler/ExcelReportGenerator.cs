using System.Text;
using ClosedXML.Attributes;
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
        try
        {
            using var wb = new XLWorkbook();

            AddSummaryWorksheet(wb, results);

            foreach (var sheet in Sheets)
            {
                AddResultWorksheet(wb, sheet, results.Scenes);;
            }

            wb.SaveAs(path);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to generate excel report");
        }
    }

    private static void AddSummaryWorksheet(XLWorkbook wb, BatchResults results)
    {
        ReadOnlySpan<int> percentageColumns = [3, 5, 7, 9, 11];
        
        var ws = wb.Worksheets.Add("Summary");
        ws.ColumnWidth = 12;
        
        var summarized = SummarizeResults(results);
        
        var target = ws.FirstCell();
        target.Value = "Summary";
        target.Style.Font.Bold = true;
        target.Style.Font.FontSize = 18;
        target = target.CellBelow();
        var table = target.InsertTable(GenerateSummaryRows(summarized), true);
        table.Sort("CPU ASC");
        
        // Title
        ws.Column(1).Width = 50;

        // Values
        ws.Column(2).Style.NumberFormat.Format = Sheets.First(s => s.Name == "CPU").NumberFormat;
        ws.Column(4).Style.NumberFormat.Format = Sheets.First(s => s.Name == "GPU").NumberFormat;
        ws.Column(6).Style.NumberFormat.Format = Sheets.First(s => s.Name == "Draw Calls").NumberFormat;
        ws.Column(8).Style.NumberFormat.Format = Sheets.First(s => s.Name == "Scene Triangles").NumberFormat;
        ws.Column(10).Style.NumberFormat.Format = Sheets.First(s => s.Name == "VRAM Usage").NumberFormat;

        // Percentages
        foreach (var col in percentageColumns)
        {
            var style = ws.Column(col).Style;
            style.NumberFormat.Format = "[Red]0.0%;-0.0%;0.0%";
            style.Font.Italic = true;
        }
    }
    
    private static Dictionary<string, SceneResult> SummarizeResults(BatchResults batchResults)
    {
        var groupedResults = new Dictionary<string, SampleHolder>();
        var singleTrack = batchResults.Scenes.Values.First().Results
            .DistinctBy(r => $"{r.TrackName}-{r.TrackLayout}").Count() == 1;

        foreach (var scene in batchResults.Scenes.Values)
        {
            foreach (var result in scene.Results)
            {
                var rowName = GenerateRowName(result, singleTrack);
            
                if (!groupedResults.TryGetValue(rowName, out var existingResult))
                {
                    groupedResults[rowName] = new SampleHolder
                    {
                        CpuTimeMs = new List<double>(result.Result.Samples.CpuTimeMs),
                        GpuTimeMs = new List<double>(result.Result.Samples.GpuTimeMs),
                        DrawCalls = new List<double>(result.Result.Samples.DrawCalls),
                        SceneTriangles = new List<double>(result.Result.Samples.SceneTriangles),
                        VramUsage = new List<double>(result.Result.Samples.VramUsage)
                    };
                }
                else
                {
                    existingResult.CpuTimeMs.AddRange(result.Result.Samples.CpuTimeMs);
                    existingResult.GpuTimeMs.AddRange(result.Result.Samples.GpuTimeMs);
                    existingResult.DrawCalls.AddRange(result.Result.Samples.DrawCalls);
                    existingResult.SceneTriangles.AddRange(result.Result.Samples.SceneTriangles);
                    existingResult.VramUsage.AddRange(result.Result.Samples.VramUsage);
                }
            }
        }

        return groupedResults.ToDictionary(result => result.Key, result => new SceneResult(result.Value));
    }

    private static void AddResultWorksheet(XLWorkbook wb, ExcelSheet sheet, Dictionary<string, BatchSceneResults> scenes)
    {
        ReadOnlySpan<int> valueColumns = [2, 4, 6, 8, 10, 12, 14, 16];
        ReadOnlySpan<int> percentageColumns = [3, 5, 7, 9, 11, 13, 15, 17];
        
        var ws = wb.Worksheets.Add(sheet.Name);
        ws.ColumnWidth = 12;

        var target = ws.FirstCell();
        foreach (var scene in scenes)
        {
            target.Value = scene.Key;
            target.Style.Font.Bold = true;
            target.Style.Font.FontSize = 18;
            target = target.CellBelow();
            var table = target.InsertTable(GenerateResultRows(scene.Value.Results, sheet.Accessor), true);
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
    
    private static List<ExcelSummaryRow> GenerateSummaryRows(Dictionary<string, SceneResult> results)
    {
        var ret = new List<ExcelSummaryRow>();
        
        var baseline = results.First().Value;
        foreach (var result in results)
        {
            ret.Add(new ExcelSummaryRow(result.Key,
                result.Value.CpuTimeStatistics.Average,
                GetIncrease(baseline.CpuTimeStatistics.Average, result.Value.CpuTimeStatistics.Average),
                result.Value.GpuTimeStatistics.Average,
                GetIncrease(baseline.GpuTimeStatistics.Average, result.Value.GpuTimeStatistics.Average),
                result.Value.DrawCallsStatistics.Average,
                GetIncrease(baseline.DrawCallsStatistics.Average, result.Value.DrawCallsStatistics.Average),
                result.Value.SceneTrianglesStatistics.Average,
                GetIncrease(baseline.SceneTrianglesStatistics.Average, result.Value.SceneTrianglesStatistics.Average),
                result.Value.VramUsageStatistics.Average,
                GetIncrease(baseline.VramUsageStatistics.Average, result.Value.VramUsageStatistics.Average)));
        }

        return ret;
    }


    private static List<ExcelPerformanceRow> GenerateResultRows(List<BatchSceneResult> results,
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
internal record ExcelSummaryRow(
    [property: XLColumn(Header = "Name")] string Name,
    [property: XLColumn(Header = "CPU")] double CPU,
    [property: XLColumn(Header = "CPU ±")] double CPUP,
    [property: XLColumn(Header = "GPU")] double GPU,
    [property: XLColumn(Header = "GPU ±")] double GPUP,
    [property: XLColumn(Header = "Draw Calls")] double DrawCalls,
    [property: XLColumn(Header = "Draw Calls ±")] double DrawCallsP,
    [property: XLColumn(Header = "Triangles")]double SceneTriangles,
    [property: XLColumn(Header = "Triangles ±")] double SceneTrianglesP,
    [property: XLColumn(Header = "VRAM")] double VRAMUsage,
    [property: XLColumn(Header = "VRAM ±")] double VRAMUsageP);
internal record ExcelPerformanceRow(string Name, double Avg, double AvgP, double Min, double MinP, double Max, double MaxP, double StdDev, double StdDevP, double P50, double P50P, double P75, double P75P, double P90, double P90P, double P99, double P99P);