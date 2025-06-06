﻿using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;
using DotNext.Runtime.InteropServices;
using Serilog;

namespace Assetto.PerformanceProfiler;

public class PerformanceRecorder
{
    private readonly MemoryMappedFile _file;
    private readonly IMappedMemory _accessor;
    private readonly Pointer<PerformanceProfilerMappedFile> _pointer;

    private ref PerformanceProfilerMappedFile CurrentValues => ref _pointer.Value;

    private PerformanceRecorder(MemoryMappedFile file)
    {
        _file = file;
        _accessor = _file.CreateMemoryAccessor();
        _pointer = new Pointer<PerformanceProfilerMappedFile>(_accessor.Pointer.Address);
    }
    
    public static bool TryOpen([NotNullWhen(true)] out PerformanceRecorder? recorder)
    {
        try
        {
            recorder = new PerformanceRecorder(MemoryMappedFile.OpenExisting("Assetto.PerformanceProfiler.v1", MemoryMappedFileRights.ReadWrite));
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

    public List<SampleHolder> Record(CancellationToken token = default)
    {
        ref var values = ref CurrentValues;

        var sceneResults = new List<SampleHolder>();
        
        values.Reset = 1;
        long lastCtr = 0;
        int lastSceneId = 0;
        while (!token.IsCancellationRequested)
        {
            if (values.Reset == 0 && values.Counter > lastCtr)
            {
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

            Thread.Sleep(1);
        }

        if (token.IsCancellationRequested && sceneResults.Count > 0)
        {
            // Discard incomplete samples
            sceneResults.RemoveAt(sceneResults.Count - 1);
        }
        
        return sceneResults;
    }
}
