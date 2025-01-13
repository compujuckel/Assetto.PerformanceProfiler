using Assetto.PerformanceMeter.Razor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Assetto.PerformanceMeter;

public class MainService : BackgroundService
{
    private readonly HtmlRenderer _renderer;
    private readonly SystemInfoService _systemInfoService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly Configuration _configuration;

    public MainService(HtmlRenderer renderer,
        IHostApplicationLifetime applicationLifetime,
        SystemInfoService systemInfoService,
        Configuration configuration)
    {
        _renderer = renderer;
        _applicationLifetime = applicationLifetime;
        _systemInfoService = systemInfoService;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var runConfig in _configuration.GetRunConfigurations())
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
            var results = recorder.Record();

            Log.Information("Shutting down Assetto Corsa...");
            await assettoCts.CancelAsync();
            await assettoTask;


            Log.Information("Generating report...");
            var html = await _renderer.Dispatcher.InvokeAsync(async () =>
            {
                var dictionary = new Dictionary<string, object?>
                {
                    { "Results", new Results(runConfig, results, _systemInfoService.GetSystemInfo()) },
                };

                var parameters = ParameterView.FromDictionary(dictionary);
                var output = await _renderer.RenderComponentAsync<ResultPage>(parameters);

                return output.ToHtmlString();
            });

            await File.WriteAllTextAsync($"results_{runConfig.TrackName}-{runConfig.TrackLayout}_{runConfig.CarModel}-{runConfig.CarSkin}.html", html, stoppingToken);
        }

        _applicationLifetime.StopApplication();
    }
}
