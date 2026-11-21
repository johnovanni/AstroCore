namespace AstroCore.Bodies;

/// <summary>Ready-made rotation models (see <see cref="IauRotationElements"/> for the sources of the IAU elements).</summary>
public static class BodyRotations
{
    public static EarthRotationModel Earth { get; } = new();
    public static IauRotationModel Sun { get; } = new(IauRotationElements.Sun);
    public static IauRotationModel Moon { get; } = new(IauRotationElements.Moon);
    public static IauRotationModel Mars { get; } = new(IauRotationElements.Mars);
}
