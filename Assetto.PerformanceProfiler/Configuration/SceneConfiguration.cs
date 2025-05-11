using System.Numerics;
using JetBrains.Annotations;

namespace Assetto.PerformanceProfiler.Configuration;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class SceneConfiguration
{
    public required string Name { get; init; }
    public required CameraMode CameraMode { get; init; }
    public DrivableCamera? DrivableCamera { get; init; }
    public Vector3? CameraPosition { get; init; }
    public Vector3? CameraLook { get; init; }
    public Vector3? CameraUp { get; init; }
    public int? CameraFOV { get; init; }
    public int DurationSeconds { get; init; }
}
