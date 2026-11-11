using AstroCore.Astronomy;
using AstroCore.Earth;
using Xunit.Abstractions;

namespace AstroCore.Tests;

public class SubPointsTests
{
    private readonly ITestOutputHelper _out;
    public SubPointsTests(ITestOutputHelper output) => _out = output;

    private const double AuKm = 149_597_870.7;
    private static DateTime U(int y, int mo, int d, int h) => new(y, mo, d, h, 0, 0, DateTimeKind.Utc);

    private static double SeparationDeg(double ra1, double dec1, double ra2, double dec2)
    {
        var a = new GeoPosition(dec1, ra1).ToUnitVector(); var b = new GeoPosition(dec2, ra2).ToUnitVector();
        return Math.Acos(Math.Clamp(Vector3d.Dot(a, b), -1, 1)) * 180 / Math.PI;
    }

    public static TheoryData<DateTime, double, double, double> SunRef => new()
    {
        { U(2026, 3, 20, 12), 359.894843697, -0.045488014, 0.99588568349617 },
        { U(2026, 6, 21, 12),  90.155654652, 23.437851309, 1.01620274452142 },
        { U(2026, 11, 10, 0), 225.136682593, -17.081149295, 0.99048012970480 },
        { U(2026, 12, 21, 12), 269.591437110, -23.436888513, 0.98375767705765 },
        { U(2030, 8, 15, 6), 144.834759588, 14.017147012, 1.01284581485577 },
    };

    public static TheoryData<DateTime, double, double, double, double, bool> MoonRef => new()
    {   // utc, RA, Dec, distance AU, illuminated %, waxing (Horizons "/T" = waxing, "/L" = waning)
        { U(2026, 3, 20, 12),  16.105368480, 10.503940182, 0.00246670500831,  2.81470, true },
        { U(2026, 6, 21, 12), 174.810347252,  0.049117817, 0.00258093733582, 45.83584, true },
        { U(2026, 11, 10, 0), 231.981249329, -24.008225458, 0.00267233043600,  0.68044, true },
        { U(2026, 12, 21, 12), 49.786880385, 23.602048833, 0.00243584025601, 90.29804, true },
        { U(2030, 8, 15, 6), 344.757169571, -1.037713239, 0.00254912192185, 95.84164, false },
    };

    [Theory, MemberData(nameof(SunRef))]
    public void SunMatchesHorizons(DateTime utc, double ra, double dec, double au)
    {
        var s = SubPoints.Sun(utc);
        double sep = SeparationDeg(s.RightAscensionDeg, s.DeclinationDeg, ra, dec);
        _out.WriteLine($"SUN  {utc:yyyy-MM-dd HH} sep={sep:F5} deg, dist err={(s.DistanceKm - au * AuKm) / (au * AuKm) * 100:F4}%");
        Assert.True(sep <= 0.05);
        Assert.True(Math.Abs(s.DistanceKm - au * AuKm) / (au * AuKm) <= 0.002);
    }

    [Theory, MemberData(nameof(MoonRef))]
    public void MoonMatchesHorizons(DateTime utc, double ra, double dec, double au, double illumPct, bool waxing)
    {
        var m = SubPoints.Moon(utc);
        double sep = SeparationDeg(m.RightAscensionDeg, m.DeclinationDeg, ra, dec);
        double ill = SubPoints.MoonIlluminatedFraction(utc) * 100;
        _out.WriteLine($"MOON {utc:yyyy-MM-dd HH} sep={sep:F5} deg, dist err={(m.DistanceKm - au * AuKm) / (au * AuKm) * 100:F4}%, illum={ill:F5}% (Horizons {illumPct}%)");
        Assert.True(sep <= 0.05);
        Assert.True(Math.Abs(m.DistanceKm - au * AuKm) / (au * AuKm) <= 0.002);
        Assert.InRange(ill - illumPct, -0.5, 0.5);
        Assert.Equal(waxing, SubPoints.MoonIsWaxing(utc));
    }

    [Fact]
    public void SubSolarPointIsDeclinationAndRaMinusGmst()
    {
        var t = U(2026, 6, 21, 12);
        var s = SubPoints.Sun(t); var p = SubPoints.SubSolarPoint(t);
        Assert.Equal(s.DeclinationDeg, p.LatitudeDeg, 9);
        Assert.Equal(GeoPosition.NormalizeLongitude(s.RightAscensionDeg - EarthRotation.GreenwichMeanSiderealTimeDeg(t)), p.LongitudeDeg, 9);
    }

    [Fact] // Review Focus 2
    public void LocalTimeGivesSameAnswerAsUtc()
    {
        var utc = U(2026, 6, 21, 12);
        Assert.Equal(SubPoints.SubSolarPoint(utc), SubPoints.SubSolarPoint(utc.ToLocalTime()));
        Assert.Equal(SubPoints.SubSolarPoint(utc), SubPoints.SubSolarPoint(DateTime.SpecifyKind(utc, DateTimeKind.Unspecified)));
    }

    [Theory] // Review Focus 1
    [InlineData(180.0, 180.0)] [InlineData(-180.0, 180.0)] [InlineData(540.0, 180.0)]
    [InlineData(-179.999, -179.999)] [InlineData(359.0, -1.0)]
    public void LongitudeNormalisesToHalfOpenRange(double input, double expected) =>
        Assert.Equal(expected, GeoPosition.NormalizeLongitude(input), 9);

    [Fact]
    public void UnitVectorAxes()
    {
        var x = new GeoPosition(0, 0).ToUnitVector(); var y = new GeoPosition(0, 90).ToUnitVector(); var z = new GeoPosition(90, 0).ToUnitVector();
        Assert.Equal(1, x.X, 12); Assert.Equal(1, y.Y, 12); Assert.Equal(1, z.Z, 12);
        var back = GeoPosition.FromVector(new GeoPosition(-33.9, 151.2).ToUnitVector());
        Assert.Equal(-33.9, back.LatitudeDeg, 9); Assert.Equal(151.2, back.LongitudeDeg, 9);
    }
}
