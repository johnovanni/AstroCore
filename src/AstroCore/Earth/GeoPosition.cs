using AstroCore.Astronomy;

namespace AstroCore.Earth;

/// <summary>
/// Geodetic-style position on a sphere. Earth-fixed frame: +X to (0°N, 0°E), +Y to (0°N, 90°E), +Z to the north pole.
/// </summary>
public readonly record struct GeoPosition(double LatitudeDeg, double LongitudeDeg)
{
    public Vector3d ToUnitVector()
    {
        double lat = LatitudeDeg * Frames.Deg, lon = LongitudeDeg * Frames.Deg;
        double c = Math.Cos(lat);
        return new Vector3d(c * Math.Cos(lon), c * Math.Sin(lon), Math.Sin(lat));
    }

    public static GeoPosition FromVector(Vector3d v)
    {
        double len = v.Length;
        double lat = Math.Asin(Math.Clamp(v.Z / len, -1, 1)) / Frames.Deg;
        double lon = Math.Atan2(v.Y, v.X) / Frames.Deg;
        return new GeoPosition(lat, NormalizeLongitude(lon));
    }

    /// <summary>Normalises a longitude to (-180, 180].</summary>
    public static double NormalizeLongitude(double lonDeg)
    {
        double x = lonDeg % 360.0;           // (-360, 360)
        if (x <= -180.0) x += 360.0;
        else if (x > 180.0) x -= 360.0;
        return x;
    }
}

/// <summary>Geocentric position in the mean equator of date.</summary>
public readonly record struct SkyPosition(double RightAscensionDeg, double DeclinationDeg, double DistanceKm);
