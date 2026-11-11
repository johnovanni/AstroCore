namespace AstroCore.Orbits;

/// <summary>The Earth–Moon system: barycentre elements, geocentric Moon and heliocentric Earth.</summary>
public static class EarthMoonSystem
{
    /// <summary>Earth–Moon barycentre, JPL approximate Keplerian elements (Standish), 1800–2050.</summary>
    public static PlanetOrbit EarthMoonBarycentre { get; } = new(
        1.00000261, 0.00000562, 0.01671123, -0.00004392, -0.00001531, -0.01294668,
        100.46457166, 35999.37244981, 102.93768193, 0.32327364, 0.0, 0.0);

    /// <summary>Geocentric Moon (Meeus ch. 47 principal terms).</summary>
    public static LunarOrbit Moon { get; } = new();

    /// <summary>Heliocentric Earth: barycentre minus the Moon's offset.</summary>
    public static EarthOrbit Earth { get; } = new(EarthMoonBarycentre, Moon);
}
