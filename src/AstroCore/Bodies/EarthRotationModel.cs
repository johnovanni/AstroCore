using AstroCore.Astronomy;
using AstroCore.Earth;

namespace AstroCore.Bodies;

/// <summary>Earth orientation: precession of the equator plus Greenwich mean sidereal time (same arithmetic as <see cref="SubPoints"/>).</summary>
public sealed class EarthRotationModel : IBodyRotation
{
    public GeoPosition ToBodyFixed(Vector3d directionEclipticJ2000, DateTime utc)
    {
        utc = utc.Kind == DateTimeKind.Local ? utc.ToUniversalTime() : utc;
        double jd = AstroTime.ToJulianDayTT(utc);
        var eq = Matrix3d.RotationX(Frames.ObliquityJ2000) * directionEclipticJ2000;
        var d = Precession.FromJ2000(jd) * eq;
        double ra = Frames.NormalizeDegrees(Math.Atan2(d.Y, d.X) / Frames.Deg);
        double dec = Math.Asin(Math.Clamp(d.Z / d.Length, -1, 1)) / Frames.Deg;
        return new GeoPosition(dec, GeoPosition.NormalizeLongitude(ra - EarthRotation.GreenwichMeanSiderealTimeDeg(utc)));
    }
}
