using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using Assetto.PerformanceProfiler.Configuration;

namespace Assetto.PerformanceProfiler;

public class MainService : BackgroundService
{
    private readonly SystemInfoService _systemInfoService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly MainConfiguration _configuration;
    private readonly ExcelReportGenerator _reportGenerator;

    public MainService(IHostApplicationLifetime applicationLifetime,
        SystemInfoService systemInfoService,
        MainConfiguration configuration, 
        ExcelReportGenerator reportGenerator)
    {
        _applicationLifetime = applicationLifetime;
        _systemInfoService = systemInfoService;
        _configuration = configuration;
        _reportGenerator = reportGenerator;
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
            Log.Information("Launching Assetto Corsa...");
            var launcher = new ACLauncher();
            launcher.WriteAppConfiguration(new AppConfiguration
            {
                CurrentRun = i,
                TotalRuns = runConfigurations.Count,
                Scenes = runConfig.Scenes
            });
            using var assettoCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var assettoTask = launcher.RunAndWaitAsync(runConfig.TrackName, runConfig.TrackLayout, runConfig.CarModel, runConfig.CarSkin, assettoCts.Token);

            Log.Information("Connecting to Assetto Corsa...");
            PerformanceRecorder recorder;
            using (var sharedMemCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
            {
                sharedMemCts.CancelAfter(30_000);
                recorder = await PerformanceRecorder.TryOpenAsync(sharedMemCts.Token);
            }

            Log.Information("Recording...");
            var samples = recorder.Record(assettoCts.Token);

            Log.Information("Shutting down Assetto Corsa...");
            await assettoCts.CancelAsync();
            await assettoTask;
            
            var result = new Results(runConfig, samples, systemInfo);
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
