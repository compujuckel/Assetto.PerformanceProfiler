using JetBrains.Annotations;

namespace Assetto.PerformanceProfiler.Configuration;

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
