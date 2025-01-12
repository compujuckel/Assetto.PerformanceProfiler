using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Assetto.PerformanceMeter;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Async(a => a.Console())
            .CreateLogger();
        
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<PerformanceRecordingService>();
        builder.Services.AddSerilog();
        builder.Services.AddTransient<HtmlRenderer>();
        
        var host = builder.Build();
        await host.RunAsync();
    }
}