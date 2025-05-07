using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;
using DotNext.Runtime.InteropServices;
using Serilog;

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

    public static async Task<PerformanceRecorder> TryOpenAsync(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            if (TryOpen(out var recorder))
            {
                return recorder;
            }

            await Task.Delay(1000, token);
        }
        
        token.ThrowIfCancellationRequested();
        return null!;
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
                Log.Debug("CTR: {Counter} CPU: {CpuTimeMs} GPU: {GpuTimeMs}", values.Counter, values.CpuTimeMs, values.GpuTimeMs);
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

            Thread.Yield();
        }
        
        return sceneResults;
    }
}
