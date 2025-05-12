using Assetto.PerformanceProfiler.Configuration;
using Serilog;

namespace Assetto.PerformanceProfiler;

public class ProfilerRun : IDisposable
{
    private readonly ACLauncher _launcher;
    
    private readonly int _runIndex;
    private readonly int _totalRuns;
    private readonly RunConfiguration _configuration;
    
    public delegate ProfilerRun ProfilerRunFactory(int runIndex, int totalRuns, RunConfiguration configuration);

    private string? _existingCarFolder;
    private bool _hasRenamedCarFolder;

    public ProfilerRun(int runIndex, int totalRuns, RunConfiguration configuration, ACLauncher launcher)
    {
        _configuration = configuration;
        _launcher = launcher;
        _runIndex = runIndex;
        _totalRuns = totalRuns;
    }

    public async Task<Results> RunAsync(CancellationToken token = default)
    {
        SetupRun();
        
        Log.Information("Launching Assetto Corsa...");
        _launcher.WriteAppConfiguration(new AppConfiguration
        {
            CurrentRun = _runIndex,
            TotalRuns = _totalRuns,
            Scenes = _configuration.Scenes
        });
        using var assettoCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var assettoTask = _launcher.RunAndWaitAsync(_configuration.TrackName,
            _configuration.TrackLayout,
            _configuration.CarRenameTo ?? _configuration.CarModel,
            _configuration.CarSkin,
            assettoCts.Token);

        Log.Information("Connecting to Assetto Corsa...");
        PerformanceRecorder recorder;
        using (var sharedMemCts = CancellationTokenSource.CreateLinkedTokenSource(token))
        {
            sharedMemCts.CancelAfter(30_000);
            recorder = await PerformanceRecorder.TryOpenAsync(sharedMemCts.Token);
        }

        Log.Information("Recording...");
        var samples = recorder.Record(assettoCts.Token);

        Log.Information("Shutting down Assetto Corsa...");
        await assettoCts.CancelAsync();
        await assettoTask;
            
        return new Results(_configuration, samples);
    }
    
    private void SetupRun()
    {
        if (_configuration.CarRenameTo != null)
        {
            var carsFolder = Path.Join(_launcher.GetRootDirectory(), "content", "cars");
            
            var renamedPath = Path.Join(carsFolder, _configuration.CarRenameTo);
            if (Directory.Exists(renamedPath))
            {
                var existingCarFolder = Path.Join(carsFolder, $"{_configuration.CarRenameTo}_{Guid.NewGuid()}");
                Directory.Move(renamedPath, existingCarFolder);
                _existingCarFolder = existingCarFolder;
            }
            
            var originalPath = Path.Join(carsFolder, _configuration.CarModel);
            Directory.Move(originalPath, renamedPath);
            _hasRenamedCarFolder = true;
        }
    }

    public void Dispose()
    {
        if (_configuration.CarRenameTo != null)
        {
            var carsFolder = Path.Join(_launcher.GetRootDirectory(), "content", "cars");
            
            var renamedPath = Path.Join(carsFolder, _configuration.CarRenameTo);
            var originalPath = Path.Join(carsFolder, _configuration.CarModel);

            if (_hasRenamedCarFolder)
            {
                Directory.Move(renamedPath, originalPath);
            }

            if (!string.IsNullOrWhiteSpace(_existingCarFolder))
            {
                Directory.Move(_existingCarFolder, renamedPath);
            }
        }
    }
}
