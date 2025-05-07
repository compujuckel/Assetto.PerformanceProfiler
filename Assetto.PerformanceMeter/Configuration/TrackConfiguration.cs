using JetBrains.Annotations;

namespace Assetto.PerformanceMeter.Configuration;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class TrackConfiguration
{
    public required string Name { get; init; }
    public required string Layout { get; init; } = "";
}
