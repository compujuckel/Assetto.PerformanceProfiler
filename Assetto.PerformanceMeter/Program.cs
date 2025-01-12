using System.Runtime.InteropServices;

namespace Assetto.PerformanceMeter;

class Program
{
    static void Main(string[] args)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Only Windows");
            return;
        }

        Console.WriteLine("Hello, World!");

        if (!PerformanceRecorder.TryOpen(out var recorder))
        {
            Console.WriteLine("Could not open performance recorder");
            return;
        }

        var results = recorder.Record();

        var cpuStats = PerfStatistics.Calculate(results[0].CpuTimeMs);
        var drawCallStats = PerfStatistics.Calculate(results[0].DrawCalls);
        
        Plotting.GenerateSignalPlot(results.Select(r => r.CpuTimeMs), "CPU Time (ms)", "cputime.png");
        Plotting.GenerateSignalPlot(results.Select(r => r.GpuTimeMs), "GPU Time (ms)", "gputime.png");
        Plotting.GenerateSignalPlot(results.Select(r => r.DrawCalls), "Draw Calls", "drawcalls.png");
        Plotting.GenerateSignalPlot(results.Select(r => r.SceneTriangles), "Scene Triangles", "triangles.png");
        Plotting.GenerateSignalPlot(results.Select(r => r.VramUsage), "VRAM Usage (MB)", "vram.png");
    }
}