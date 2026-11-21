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

    [Fact]
    public void RadiiMatchIau2015()
    {
        Assert.Equal((695700.0, 695700.0), BodyConstants.Radii("sun"));
        Assert.Equal((1737.4, 1737.4), BodyConstants.Radii("moon"));
        Assert.Equal((6378.1366, 6356.7519), BodyConstants.Radii("earth"));
        Assert.Equal((3396.19, 3376.20), BodyConstants.Radii("mars"));
    }

    [Fact]
    public void UnknownBodyRadiiThrow() => Assert.Throws<ArgumentException>(() => BodyConstants.Radii("pluto"));

    [Theory]
    [InlineData("sun")]
    [InlineData("moon")]
    [InlineData("mars")]
    public void IauPoleDirectionMapsToNorthPole(string body)
    {
        // At J2000.0 (d = T = 0) the IAU pole is alpha0, delta0 from the polynomial plus the periodic terms at d = 0.
        var e = body switch { "sun" => IauRotationElements.Sun, "moon" => IauRotationElements.Moon, _ => IauRotationElements.Mars };
        double a0 = e.Alpha0.C0, d0 = e.Delta0.C0;
        foreach (var p in e.PeriodicTerms)
        {
            a0 += p.AlphaAmp * Math.Sin(p.Phase * Frames.Deg);
            d0 += p.DeltaAmp * Math.Cos(p.Phase * Frames.Deg);
        }
        var j2000Utc = new DateTime(2000, 1, 1, 11, 58, 56, DateTimeKind.Utc); // TT 12:00
        var rot = new IauRotationModel(e);
        var pos = rot.ToBodyFixed(Frames.PoleVector(a0, d0), j2000Utc);
        Assert.Equal(90.0, pos.LatitudeDeg, 4);
    }
}
