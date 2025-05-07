using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using Assetto.PerformanceMeter.Configuration;

namespace Assetto.PerformanceMeter;

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
            var assettoCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var assettoTask = launcher.RunAndWaitAsync(runConfig.TrackName, runConfig.TrackLayout, runConfig.CarModel, runConfig.CarSkin, assettoCts.Token);

            Log.Information("Connecting to Assetto Corsa...");
            var sharedMemCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            sharedMemCts.CancelAfter(300000);
            var recorder = await PerformanceRecorder.TryOpenAsync(sharedMemCts.Token);

            Log.Information("Recording...");
            var samples = recorder.Record();

            Log.Information("Shutting down Assetto Corsa...");
            await assettoCts.CancelAsync();
            await assettoTask;


            Log.Information("Generating report...");
            var result = new Results(runConfig, samples, systemInfo);
            batchResults.AddResult(result);

            i++;
        }

        await using (var jsonFile = File.Create("batch_results.json"))
        {
            await JsonSerializer.SerializeAsync(jsonFile, batchResults, JsonSerializerOptions.Default, stoppingToken);
        }

        _reportGenerator.GenerateBatch(batchResults, "batch_results.xlsx");
        _applicationLifetime.StopApplication();
    }
}
