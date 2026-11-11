using AstroCore.Astronomy;
using AstroCore.Orbits;

namespace AstroCore.Tests;

// Horizons state vectors (J2000 ecliptic, AU) for 2026-11-10 00:00 TDB, JD 2461354.5 — same values SolarSystem3D tests use.
public class EarthMoonAccuracyTests
{
    private const double Jd = 2461354.5;
    private static double AngleDeg(Vector3d a, Vector3d b) =>
        Math.Acos(Math.Clamp(Vector3d.Dot(a.Normalized(), b.Normalized()), -1, 1)) / Frames.Deg;

    [Fact]
    public void EarthMatchesHorizons()
    {
        var reference = new Vector3d(6.726227034059016E-01, 7.270693863771039E-01, -4.613446748933096E-05);
        var p = EarthMoonSystem.Earth.PositionAt(Jd);
        Assert.True(AngleDeg(p, reference) <= 0.1);
        Assert.True(Math.Abs(p.Length - reference.Length) / reference.Length <= 0.002);
    }

    [Fact]
    public void MoonMatchesHorizons()
    {
        var reference = new Vector3d(-1.518324045628562E-03, -2.186718077083572E-03, -2.324870968031709E-04);
        var p = EarthMoonSystem.Moon.PositionAt(Jd);
        Assert.True(AngleDeg(p, reference) <= 0.05);
        Assert.True(Math.Abs(p.Length - reference.Length) / reference.Length <= 0.002);
    }

    [Fact]
    public void RotationYIsActiveRightHanded()
    {
        var v = Matrix3d.RotationY(Math.PI / 2) * new Vector3d(0, 0, 1);
        Assert.Equal(1, v.X, 12); Assert.Equal(0, v.Y, 12); Assert.Equal(0, v.Z, 12);
    }
}
