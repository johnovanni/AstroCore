using AstroCore.Astronomy;

namespace AstroCore.Earth;

/// <summary>Earth's rotation angle and related quantities.</summary>
public static class EarthRotation
{
    /// <summary>
    /// Greenwich mean sidereal time in degrees, in [0, 360) (Meeus eq. 12.4). UT1 is treated as UTC.
    /// </summary>
    public static double GreenwichMeanSiderealTimeDeg(DateTime utc)
    {
        double jd = AstroTime.ToJulianDayUtc(utc);
        double t = (jd - AstroTime.J2000) / AstroTime.DaysPerJulianCentury;
        double theta0 = 280.46061837
                        + 360.98564736629 * (jd - AstroTime.J2000)
                        + 0.000387933 * t * t
                        - t * t * t / 38710000.0;
        return Frames.NormalizeDegrees(theta0);
    }
}
