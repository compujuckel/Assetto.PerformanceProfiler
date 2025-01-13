using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Assetto.PerformanceMeter;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class Configuration
{
    public required List<TrackConfiguration> Tracks { get; init; }
    public required List<CarConfiguration> Cars { get; init; }
    public required List<SceneConfiguration> Scenes { get; init; }
    
    public static Configuration FromFile(string path)
    {
        using var stream = File.OpenText(path);
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Configuration>(stream);
    }

    public IEnumerable<RunConfiguration> GetRunConfigurations()
    {
        return Tracks.SelectMany(_ => Cars,
            (track, car) => new RunConfiguration
            {
                CarModel = car.Model,
                CarSkin = car.Skin,
                TrackName = track.Name,
                TrackLayout = track.Layout,
                Scenes = Scenes
            });
    }
}

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class TrackConfiguration
{
    public required string Name { get; init; }
    public required string Layout { get; init; } = "";
}

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class CarConfiguration
{
    public required string Model { get; init; }
    public required string Skin { get; init; } = "";
}

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class SceneConfiguration
{
    public required string Name { get; init; }
    public required CameraMode CameraMode { get; init; }
    public DrivableCamera? DrivableCamera { get; init; }
    public int DurationSeconds { get; init; }
}

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public enum CameraMode
{
    Cockpit = 0,
    Car = 1,
    Drivable = 2,
    Track = 3,
    Helicopter = 4,
    OnBoardFree = 5,
    Free = 6,
    Deprecated = 7,
    ImageGeneratorCamera = 8,
    Start = 9,
}

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public enum DrivableCamera
{
    Chase = 0, 
    Chase2 = 1,
    Bonnet = 2,
    Bumper = 3,
    Dash = 4,
}