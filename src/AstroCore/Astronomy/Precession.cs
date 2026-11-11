namespace AstroCore.Astronomy;

/// <summary>Precession of the equator and equinox from J2000 to a given date (Meeus, eq. 21.2).</summary>
public static class Precession
{
    private static double ArcsecToRad(double arcsec) => arcsec / 3600.0 * Frames.Deg;

    /// <summary>
    /// Rotation mapping J2000 mean equatorial vectors to the mean equator and equinox of date.
    /// </summary>
    public static Matrix3d FromJ2000(double jdTT)
    {
        double t = AstroTime.CenturiesSinceJ2000(jdTT);

        double zeta = 2306.2181 * t + 0.30188 * t * t + 0.017998 * t * t * t;
        double z = 2306.2181 * t + 1.09468 * t * t + 0.018203 * t * t * t;
        double theta = 2004.3109 * t - 0.42665 * t * t - 0.041833 * t * t * t;

        // Active-rotation form of R3(-z) * R2(theta) * R3(-zeta).
        return Matrix3d.RotationZ(ArcsecToRad(z))
             * Matrix3d.RotationY(-ArcsecToRad(theta))
             * Matrix3d.RotationZ(ArcsecToRad(zeta));
    }

    /// <summary>
    /// Converts J2000 equatorial RA/Dec (degrees) to RA/Dec of the equator and equinox of date (degrees).
    /// RA is returned in [0, 360).
    /// </summary>
    public static (double RaDeg, double DecDeg) ToEquatorOfDate(double raJ2000Deg, double decJ2000Deg, double jdTT)
    {
        double a = raJ2000Deg * Frames.Deg, d = decJ2000Deg * Frames.Deg;
        var v = new Vector3d(Math.Cos(d) * Math.Cos(a), Math.Cos(d) * Math.Sin(a), Math.Sin(d));
        var p = FromJ2000(jdTT) * v;

        double ra = Frames.NormalizeDegrees(Math.Atan2(p.Y, p.X) / Frames.Deg);
        double dec = Math.Atan2(p.Z, Math.Sqrt(p.X * p.X + p.Y * p.Y)) / Frames.Deg;
        return (ra, dec);
    }
}
