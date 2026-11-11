using AstroCore.Astronomy;
using AstroCore.Earth;

namespace AstroCore.Tests;

public class TerminatorTests
{
    [Fact]
    public void OverheadAtSubSolarPoint() => Assert.Equal(90, Terminator.SunAltitudeDeg(new(10, 20), 10, 20), 9);

    [Fact]
    public void ZeroOnGreatCircle90DegreesAway()
    {   // sub-solar at (10°N, 20°E); walk the great circle perpendicular to it in 15° steps
        var sub = new GeoPosition(10, 20);
        var s = sub.ToUnitVector();
        var a = Vector3d.Cross(s, new Vector3d(0, 0, 1)).Normalized();
        var b = Vector3d.Cross(s, a);
        for (int k = 0; k < 24; k++)
        {
            double t = k * 15 * Math.PI / 180;
            var p = GeoPosition.FromVector(a * Math.Cos(t) + b * Math.Sin(t));
            Assert.InRange(Terminator.SunAltitudeDeg(sub, p.LatitudeDeg, p.LongitudeDeg), -0.01, 0.01);
        }
    }

    [Fact]
    public void Minus6AtLongitude96OnEquatorWithSunAtOrigin() =>
        Assert.Equal(-6.0, Terminator.SunAltitudeDeg(new(0, 0), 0, 96), 2);
}
