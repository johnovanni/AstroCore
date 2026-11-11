using AstroCore.Astronomy;
using AstroCore.Orbits;

namespace AstroCore.Earth;

/// <summary>Positions of the Sun and Moon as seen from Earth, and the points on Earth directly beneath them.</summary>
public static class SubPoints
{
    private const double AuKm = 149_597_870.7;

    // Local is converted to UTC; Unspecified is treated as UTC.
    private static DateTime ToUtc(DateTime t) => t.Kind == DateTimeKind.Local ? t.ToUniversalTime() : t;

    private static Vector3d SunGeocentricEcliptic(double jdTT) => -EarthMoonSystem.Earth.PositionAt(jdTT);

    private static SkyPosition ToSky(Vector3d eclipticAu, double jdTT)
    {
        var eq = Matrix3d.RotationX(Frames.ObliquityJ2000) * eclipticAu;
        var d = Precession.FromJ2000(jdTT) * eq;
        double ra = Frames.NormalizeDegrees(Math.Atan2(d.Y, d.X) / Frames.Deg);
        double len = d.Length;
        double dec = Math.Asin(Math.Clamp(d.Z / len, -1, 1)) / Frames.Deg;
        return new SkyPosition(ra, dec, len * AuKm);
    }

    public static SkyPosition Sun(DateTime utc)
    {
        utc = ToUtc(utc);
        double jd = AstroTime.ToJulianDayTT(utc);
        return ToSky(SunGeocentricEcliptic(jd), jd);
    }

    public static SkyPosition Moon(DateTime utc)
    {
        utc = ToUtc(utc);
        double jd = AstroTime.ToJulianDayTT(utc);
        return ToSky(EarthMoonSystem.Moon.PositionAt(jd), jd);
    }

    public static GeoPosition SubSolarPoint(DateTime utc) => SubPoint(Sun(ToUtc(utc)), ToUtc(utc));

    public static GeoPosition SubLunarPoint(DateTime utc) => SubPoint(Moon(ToUtc(utc)), ToUtc(utc));

    private static GeoPosition SubPoint(SkyPosition p, DateTime utc) =>
        new(p.DeclinationDeg, GeoPosition.NormalizeLongitude(p.RightAscensionDeg - EarthRotation.GreenwichMeanSiderealTimeDeg(utc)));

    /// <summary>Illuminated fraction of the Moon's disc, 0 to 1.</summary>
    public static double MoonIlluminatedFraction(DateTime utc)
    {
        double jd = AstroTime.ToJulianDayTT(ToUtc(utc));
        var moon = EarthMoonSystem.Moon.PositionAt(jd);
        var sun = SunGeocentricEcliptic(jd);
        var toSun = (sun - moon).Normalized();
        var toEarth = (-moon).Normalized();
        double cosI = Math.Clamp(Vector3d.Dot(toSun, toEarth), -1, 1);
        return (1 + cosI) / 2;
    }

    /// <summary>True when the Moon's ecliptic longitude is 0 to 180 degrees ahead of the Sun's (new to full).</summary>
    public static bool MoonIsWaxing(DateTime utc)
    {
        double jd = AstroTime.ToJulianDayTT(ToUtc(utc));
        var moon = EarthMoonSystem.Moon.PositionAt(jd);
        var sun = SunGeocentricEcliptic(jd);
        double lm = Math.Atan2(moon.Y, moon.X) / Frames.Deg;
        double ls = Math.Atan2(sun.Y, sun.X) / Frames.Deg;
        double elong = Frames.NormalizeDegrees(lm - ls);
        return elong > 0 && elong < 180;
    }
}
