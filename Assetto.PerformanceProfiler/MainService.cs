using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Assetto.PerformanceProfiler.Configuration;

namespace Assetto.PerformanceProfiler;

public class MainService : BackgroundService
{
    private readonly SystemInfoService _systemInfoService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly MainConfiguration _configuration;
    private readonly ExcelReportGenerator _reportGenerator;
    private readonly ProfilerRun.ProfilerRunFactory _profilerRunFactory;

    public MainService(IHostApplicationLifetime applicationLifetime,
        SystemInfoService systemInfoService,
        MainConfiguration configuration, 
        ExcelReportGenerator reportGenerator,
        ProfilerRun.ProfilerRunFactory profilerRunFactory)
    {
        _applicationLifetime = applicationLifetime;
        _systemInfoService = systemInfoService;
        _configuration = configuration;
        _reportGenerator = reportGenerator;
        _profilerRunFactory = profilerRunFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var systemInfo = _systemInfoService.GetSystemInfo();
        var batchResults = new BatchResults(systemInfo);
        //var batchResults = JsonSerializer.Deserialize<BatchResults>(await File.ReadAllTextAsync("batch_results.json", stoppingToken))!;

        var runConfigurations = _configuration.GetRunConfigurations().ToList();
        int i = 1;
        foreach (var runConfig in runConfigurations)
        {
            using var profiler = _profilerRunFactory(i, runConfigurations.Count, runConfig);
            
            var result = await profiler.RunAsync(stoppingToken);
            batchResults.AddResult(result);

            if (stoppingToken.IsCancellationRequested) break;

            i++;
        }

        await using (var jsonFile = File.Create("batch_results.json"))
        {
            // Do not pass stoppingToken so report is still generated when closing application
            await JsonSerializer.SerializeAsync(jsonFile, batchResults, JsonSerializerOptions.Default, CancellationToken.None);
        }

        _reportGenerator.GenerateBatch(batchResults, "batch_results.xlsx");
        _applicationLifetime.StopApplication();
    }
}
