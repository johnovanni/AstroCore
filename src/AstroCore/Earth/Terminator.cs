using AstroCore.Astronomy;

namespace AstroCore.Earth;

public static class Terminator
{
    /// <summary>
    /// Altitude of the Sun in degrees at (latDeg, lonDeg), given the sub-solar point.
    /// Equal to asin(sin φ sin δ + cos φ cos δ cos Δλ); computed as 90° minus the angular distance via
    /// atan2 so it stays accurate near the zenith, where asin loses precision.
    /// </summary>
    public static double SunAltitudeDeg(GeoPosition subSolar, double latDeg, double lonDeg)
    {
        var p = new GeoPosition(latDeg, lonDeg).ToUnitVector();
        var s = subSolar.ToUnitVector();
        double dist = Math.Atan2(Vector3d.Cross(p, s).Length, Vector3d.Dot(p, s)) / Frames.Deg;
        return 90.0 - dist;
    }
}
