using System.Runtime.InteropServices;

namespace Assetto.PerformanceMeter;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PerformanceMeterMappedFile
{
    public long Counter;
    public double CpuTimeMs;
    public double GpuTimeMs;
    public double VramUsage;
    public int DrawCalls;
    public int SceneTriangles;
    public int Lights;
    public int ExtraShadows;
    public int Scene;
    public byte Reset;
}
