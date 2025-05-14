using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Assetto.PerformanceProfiler.Configuration;
using Assetto.PerformanceProfiler.Model;
using Serilog;

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
        //var batchResults = BatchResults.FromFile("batch_results.json");

        var startTime = DateTime.UtcNow;
        DateTime? etaTimestamp = null;
        var runConfigurations = _configuration.GetRunConfigurations().ToList();
        for (var i = 0; i < runConfigurations.Count && !stoppingToken.IsCancellationRequested; i++)
        {
            var runConfig = runConfigurations[i];

            long? etaTimestampUtc = etaTimestamp.HasValue ? new DateTimeOffset(etaTimestamp.Value).ToUnixTimeSeconds() : null;
            using var profiler = _profilerRunFactory(i + 1, runConfigurations.Count, etaTimestampUtc, runConfig);

            var result = await profiler.RunAsync(stoppingToken);
            batchResults.AddRunResult(result);
            
            var timeElapsed = DateTime.UtcNow - startTime;
            var timeRemaining = TimeSpan.FromSeconds(timeElapsed.TotalSeconds / (i + 1) * (runConfigurations.Count - i - 1));
            etaTimestamp = DateTime.Now + timeRemaining;
            
            Log.Information("Run {RunIndex}/{TotalRuns} completed. Estimated finish time: {EtaTimestamp}", i + 1, runConfigurations.Count, etaTimestamp);
        }

        batchResults.ToFile("batch_results.json");

        _reportGenerator.GenerateBatch(batchResults, "batch_results.xlsx");
        _applicationLifetime.StopApplication();
    }
}
