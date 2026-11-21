using AstroCore.Astronomy;
using AstroCore.Earth;
using AstroCore.Orbits;

namespace AstroCore.Bodies;

/// <summary>Sub-solar and sub-observer points for any body with a heliocentric position and a rotation model.</summary>
public static class BodySubPoints
{
    /// <summary>Speed of light in AU per day.</summary>
    private const double CAuPerDay = 173.1446326846693;

    private static DateTime ToUtc(DateTime t) => t.Kind == DateTimeKind.Local ? t.ToUniversalTime() : t;

    /// <summary>
    /// Point on the body directly beneath the Sun. <paramref name="bodyHeliocentric"/> maps jdTT to the body's
    /// heliocentric J2000 ecliptic position in AU; null means the Earth.
    /// </summary>
    public static GeoPosition SubSolarPoint(Func<double, Vector3d>? bodyHeliocentric, IBodyRotation rotation, DateTime utc)
    {
        utc = ToUtc(utc);
        double jd = AstroTime.ToJulianDayTT(utc);
        var pos = bodyHeliocentric?.Invoke(jd) ?? EarthMoonSystem.Earth.PositionAt(jd);
        return rotation.ToBodyFixed(-pos, utc);
    }

    /// <summary>
    /// Point on the body directly beneath the observer. With <paramref name="lightTime"/> the body is taken where it was
    /// when the light left it: t' = t - |B(t') - O(t)|/c, iterated twice; the rotation is evaluated at t'.
    /// </summary>
    public static GeoPosition SubObserverPoint(Func<double, Vector3d> bodyHeliocentric, IBodyRotation rotation,
        Func<double, Vector3d> observerHeliocentric, DateTime observerUtc, bool lightTime = true)
    {
        observerUtc = ToUtc(observerUtc);
        double jd = AstroTime.ToJulianDayTT(observerUtc);
        var o = observerHeliocentric(jd);
        double lt = 0; // light-time in days
        if (lightTime)
        {
            for (int i = 0; i < 2; i++)
                lt = Vector3d.Distance(bodyHeliocentric(jd - lt), o) / CAuPerDay;
        }
        var b = bodyHeliocentric(jd - lt);
        return rotation.ToBodyFixed(o - b, observerUtc.AddDays(-lt));
    }
}
