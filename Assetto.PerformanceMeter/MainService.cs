using System.Text.Json;
using Assetto.PerformanceMeter.Razor;
using Microsoft.Extensions.Hosting;

namespace Assetto.PerformanceMeter;

public class MainService : BackgroundService
{
    private readonly RazorRenderer _renderer;
    private readonly SystemInfoService _systemInfoService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly Configuration _configuration;
    private readonly ExcelReportGenerator _reportGenerator;

    public MainService(RazorRenderer renderer,
        IHostApplicationLifetime applicationLifetime,
        SystemInfoService systemInfoService,
        Configuration configuration, 
        ExcelReportGenerator reportGenerator)
    {
        _renderer = renderer;
        _applicationLifetime = applicationLifetime;
        _systemInfoService = systemInfoService;
        _configuration = configuration;
        _reportGenerator = reportGenerator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var systemInfo = _systemInfoService.GetSystemInfo();
        //var batchResults = new BatchResults(systemInfo);
        var batchResults = JsonSerializer.Deserialize<BatchResults>(await File.ReadAllTextAsync("batch_results.json", stoppingToken))!;
        
        /*foreach (var runConfig in _configuration.GetRunConfigurations())
        {
            Log.Information("Launching Assetto Corsa...");
            var launcher = new ACLauncher();
            var assettoCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var assettoTask = launcher.RunAndWaitAsync(runConfig.TrackName, runConfig.TrackLayout, runConfig.CarModel, runConfig.CarSkin, assettoCts.Token);

            Log.Information("Connecting to Assetto Corsa...");
            var sharedMemCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            sharedMemCts.CancelAfter(30000);
            var recorder = await PerformanceRecorder.TryOpenAsync(sharedMemCts.Token);

            Log.Information("Recording...");
            var samples = recorder.Record();

            Log.Information("Shutting down Assetto Corsa...");
            await assettoCts.CancelAsync();
            await assettoTask;


            Log.Information("Generating report...");
            var result = new Results(runConfig, samples, systemInfo);
            batchResults.AddResult(result);
            
            var html = await _renderer.RenderToStringAsync<ResultPage>(new { Results = result });
            
            await File.WriteAllTextAsync($"results_{runConfig.TrackName}-{runConfig.TrackLayout}_{runConfig.CarModel}-{runConfig.CarSkin}.html", html, stoppingToken);
        }*/
        
        _reportGenerator.GenerateBatch(batchResults, "batch_results.xlsx");
        
        var batchResultsHtml = await _renderer.RenderToStringAsync<BatchResultsPage>(new { Results = batchResults });
        await File.WriteAllTextAsync("batch_results.html", batchResultsHtml, stoppingToken);

        _applicationLifetime.StopApplication();
    }
}
