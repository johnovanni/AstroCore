namespace AstroCore.Bodies;

/// <summary>Body sizes.</summary>
public static class BodyConstants
{
    /// <summary>
    /// Equatorial and polar radii in km, from Archinal et al. 2018 (IAU WGCCRE 2015 report,
    /// doi:10.1007/s10569-017-9805-5): Table 4 for the Sun (695,700; a sphere), the Earth (6378.1366 / 6356.7519)
    /// and Mars (3396.19 / 3376.20, the average of the north and south polar radii), Table 5 for the Moon
    /// (1737.4 mean radius for cartographic use). Ids: "sun", "moon", "earth", "mars" (case-insensitive).
    /// </summary>
    public static (double EquatorialKm, double PolarKm) Radii(string bodyId) => bodyId.ToLowerInvariant() switch
    {
        "sun" => (695700.0, 695700.0),
        "moon" => (1737.4, 1737.4),
        "earth" => (6378.1366, 6356.7519),
        "mars" => (3396.19, 3376.20),
        _ => throw new ArgumentException($"No radii for body '{bodyId}'.", nameof(bodyId)),
    };
}
