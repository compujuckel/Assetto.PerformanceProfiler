using JetBrains.Annotations;

namespace Assetto.PerformanceMeter.Configuration;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class CarConfiguration
{
    public required string Model { get; init; }
    public required string Skin { get; init; } = "";
}
