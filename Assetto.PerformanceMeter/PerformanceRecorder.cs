using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using DotNext.IO.MemoryMappedFiles;
using DotNext.Runtime.InteropServices;

namespace Assetto.PerformanceMeter;

public class PerformanceRecorder
{
    private readonly MemoryMappedFile _file;
    private readonly IMappedMemory _accessor;
    private readonly Pointer<PerformanceMeterMappedFile> _pointer;

    private ref PerformanceMeterMappedFile CurrentValues => ref _pointer.Value;

    private PerformanceRecorder(MemoryMappedFile file)
    {
        _file = file;
        _accessor = _file.CreateMemoryAccessor();
        _pointer = new Pointer<PerformanceMeterMappedFile>(_accessor.Pointer.Address);
    }
    
    [SupportedOSPlatform("windows")]
    public static bool TryOpen([NotNullWhen(true)] out PerformanceRecorder? recorder)
    {
        try
        {
            recorder = new PerformanceRecorder(MemoryMappedFile.OpenExisting("Assetto.PerformanceMeter.v5", MemoryMappedFileRights.ReadWrite));
            return true;
        }
        catch (FileNotFoundException)
        {
            recorder = null;
            return false;
        }
    }

    public List<SampleHolder> Record()
    {
        ref var values = ref CurrentValues;

        var sceneResults = new List<SampleHolder>();
        
        values.Reset = 1;
        long lastCtr = 0;
        int lastSceneId = 0;
        while (true)
        {
            if (values.Reset == 0 && values.Counter > lastCtr)
            {
                Console.WriteLine($"CTR: {values.Counter} CPU: {values.CpuTimeMs} GPU: {values.GpuTimeMs}");
                lastCtr = values.Counter;

                var sceneId = values.Scene;

                if (sceneId < lastSceneId)
                    break;

                lastSceneId = sceneId;

                if (sceneId >= sceneResults.Count)
                {
                    sceneResults.Add(new SampleHolder());
                }

                sceneResults[sceneId].Record(in values);
            }

            Thread.Sleep(10);
        }
        
        return sceneResults;
    }
}
