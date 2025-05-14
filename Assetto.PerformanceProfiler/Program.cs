using Assetto.PerformanceProfiler.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Assetto.PerformanceProfiler;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Async(a => a.Console())
            .CreateLogger();

        var configuration = MainConfiguration.FromFile("configuration.yml");
        
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<MainService>();
        builder.Services.AddSerilog();
        builder.Services.AddSingleton<ExcelReportGenerator>();
        builder.Services.AddSingleton<SystemInfoService>();
        builder.Services.AddSingleton<ACLauncher>();
        builder.Services.AddSingleton(configuration);
        
        builder.Services.AddSingleton<ProfilerRun.ProfilerRunFactory>(sp =>
        {
            var launcher = sp.GetRequiredService<ACLauncher>();
            return (runIndex, totalRuns, etaTimestamp, config) => new ProfilerRun(runIndex, totalRuns, etaTimestamp, config, launcher);
        });
        
        var host = builder.Build();
        await host.RunAsync();
    }
}
