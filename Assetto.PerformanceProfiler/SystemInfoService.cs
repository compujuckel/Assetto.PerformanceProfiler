using Hardware.Info;

namespace Assetto.PerformanceProfiler;

public class SystemInfoService
{
    private readonly HardwareInfo _info;
    
    public SystemInfoService()
    {
        _info = new HardwareInfo();
        _info.RefreshOperatingSystem();
        _info.RefreshCPUList();
        _info.RefreshMemoryStatus();
        _info.RefreshVideoControllerList();
    }

    public SystemInfo GetSystemInfo()
    {
        return new SystemInfo
        {
            OS = new OperatingSystemInfo
            {
                Name = _info.OperatingSystem.Name,
                Version = _info.OperatingSystem.VersionString
            },
            CPU = _info.CpuList.Select(c => new CPUInfo
            {
                Name = c.Name.Trim(),
                Cores = (int)c.NumberOfCores,
                Threads = (int)c.NumberOfLogicalProcessors
            }).ToList(),
            GPU = _info.VideoControllerList.Select(g => new GPUInfo
            {
                Name = g.Name,
                VRAM = g.AdapterRAM,
                DriverVersion = g.DriverVersion
            }).ToList(),
            Memory = new MemoryInfo
            {
                Size = _info.MemoryStatus.TotalPhysical
            }
        };
    }
}
