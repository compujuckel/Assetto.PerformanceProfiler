using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Assetto.PerformanceProfiler.Configuration;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class MainConfiguration
{
    public required List<TrackConfiguration> Tracks { get; init; }
    public required List<CarConfiguration> Cars { get; init; }
    public required List<SceneConfiguration> Scenes { get; init; }
    
    public static MainConfiguration FromFile(string path)
    {
        using var stream = File.OpenText(path);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<MainConfiguration>(stream);
    }

    public IEnumerable<RunConfiguration> GetRunConfigurations()
    {
        return Tracks.SelectMany(_ => Cars,
            (track, car) => new RunConfiguration
            {
                CarModel = car.Model,
                CarSkin = car.Skin,
                CarRenameTo = car.RenameTo,
                TrackName = track.Name,
                TrackLayout = track.Layout,
                Scenes = Scenes
            });
    }
}