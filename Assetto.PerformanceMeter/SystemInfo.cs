namespace Assetto.PerformanceMeter;

public class SystemInfo
{
    public required OperatingSystemInfo OS { get; init; }
    public required List<CPUInfo> CPU { get; init; }
    public required List<GPUInfo> GPU { get; init; }
    public required MemoryInfo Memory { get; init; }
}

public class OperatingSystemInfo
{
    public required string Name { get; init; }
    public required string Version { get; init; }
}

public class CPUInfo
{
    public required string Name { get; init; }
    public int Cores { get; init; }
    public int Threads { get; init; }
}

public class GPUInfo
{
    public required string Name { get; init; }
    public ulong VRAM { get; init; }
    public required string DriverVersion { get; init; }
}

public class MemoryInfo
{
    public ulong Size { get; init; }
}