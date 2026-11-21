using AstroCore.Astronomy;
using AstroCore.Bodies;
using AstroCore.Earth;
using AstroCore.Orbits;

namespace AstroCore.Tests;

public class BodyRotationTests
{
    private static DateTime U(int y, int mo, int d, int h, int mi) => new(y, mo, d, h, mi, 0, DateTimeKind.Utc);

    public static TheoryData<DateTime> Times => new()
    {
        U(2000, 1, 1, 12, 0), U(2026, 11, 10, 0, 0), U(2026, 6, 21, 6, 30), U(2049, 12, 31, 23, 59),
    };

    [Theory]
    [MemberData(nameof(Times))]
    public void EarthModelMatchesSubPoints(DateTime t)
    {
        var expected = SubPoints.SubSolarPoint(t);
        var actual = BodySubPoints.SubSolarPoint(null, new EarthRotationModel(), t);
        Assert.Equal(expected.LatitudeDeg, actual.LatitudeDeg, 9);
        Assert.Equal(expected.LongitudeDeg, actual.LongitudeDeg, 9);
    }

    [Fact]
    public void SubObserverWithoutLightTimeIsSubPointOfObserverDirection()
    {
        var t = U(2026, 11, 10, 0, 0);
        var model = new EarthRotationModel();
        var observer = new Vector3d(1e6, 0, 0); // far along +X, at rest
        Func<double, Vector3d> earth = jd => EarthMoonSystem.Earth.PositionAt(jd);
        var actual = BodySubPoints.SubObserverPoint(earth, model, _ => observer, t, lightTime: false);

        double jd = AstroTime.ToJulianDayTT(t);
        var expected = model.ToBodyFixed(observer - earth(jd), t);
        Assert.Equal(expected.LatitudeDeg, actual.LatitudeDeg, 9);
        Assert.Equal(expected.LongitudeDeg, actual.LongitudeDeg, 9);

        // Essentially the +X direction (the Earth's ~1 AU offset is ~6e-5 deg at 1e6 AU).
        var far = model.ToBodyFixed(new Vector3d(1, 0, 0), t);
        Assert.Equal(far.LatitudeDeg, actual.LatitudeDeg, 3);
        Assert.Equal(far.LongitudeDeg, actual.LongitudeDeg, 3);
    }
}
