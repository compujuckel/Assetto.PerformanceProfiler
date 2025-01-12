using Assetto.PerformanceMeter.Razor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Assetto.PerformanceMeter;

public class PerformanceRecordingService : BackgroundService
{
    private readonly HtmlRenderer _renderer;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public PerformanceRecordingService(HtmlRenderer renderer, IHostApplicationLifetime applicationLifetime)
    {
        _renderer = renderer;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Launching Assetto Corsa...");
        var launcher = new ACLauncher();
        var cts = new CancellationTokenSource();
        var assettoTask = launcher.RunAndWaitAsync(cts.Token);

        Log.Information("Connecting to Assetto Corsa...");
        var recorder = await PerformanceRecorder.TryOpenAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);

        Log.Information("Recording...");
        var results = recorder.Record();
        
        Log.Information("Shutting down Assetto Corsa...");
        await cts.CancelAsync();
        await assettoTask;


        Log.Information("Generating report...");
        var html = await _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>
            {
                { "Results", new Results(results) }
            };

            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await _renderer.RenderComponentAsync<ResultPage>(parameters);

            return output.ToHtmlString();
        });

        await File.WriteAllTextAsync("results.html", html, stoppingToken);

        _applicationLifetime.StopApplication();
    }
}
