namespace AstroCore.Orbits;

/// <summary>Heliocentric planetary orbits (JPL approximate Keplerian elements, Standish, Table 1, 1800–2050).</summary>
public static class PlanetOrbits
{
    /// <summary>
    /// Mars. Elements copied from SolarSystem3D (src/SolarSystem3D.Core/Data/MajorBodyCatalog.cs), which takes them from
    /// JPL's "Keplerian Elements for Approximate Positions of the Major Planets" (E.M. Standish), Table 1.
    /// </summary>
    public static PlanetOrbit Mars { get; } = new(1.52371034, 0.00001847, 0.09339410, 0.00007882, 1.84969142, -0.00813131,
                                                  -4.55343205, 19140.30268499, -23.94362959, 0.44441088, 49.55953891, -0.29257343);
}
