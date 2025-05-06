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

        var configuration = Configuration.FromFile("configuration.yml");
        
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<MainService>();
        builder.Services.AddSerilog();
        builder.Services.AddTransient<HtmlRenderer>();
        builder.Services.AddSingleton<RazorRenderer>();
        builder.Services.AddSingleton<ExcelReportGenerator>();
        builder.Services.AddSingleton<SystemInfoService>();
        builder.Services.AddSingleton(configuration);
        
        var host = builder.Build();
        await host.RunAsync();
    }
}