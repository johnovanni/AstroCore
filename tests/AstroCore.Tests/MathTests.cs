using AstroCore.Astronomy;

namespace AstroCore.Tests;

public class MathTests
{
    [Theory]
    [InlineData(0.1, 0.0167)]
    [InlineData(3.0, 0.5)]
    [InlineData(-2.0, 0.97)]
    [InlineData(0.01, 0.999)]
    public void EllipticKeplerSolves(double m, double e)
    {
        double ea = Conic.SolveElliptic(m, e);
        Assert.Equal(Math.IEEERemainder(m, 2 * Math.PI), ea - e * Math.Sin(ea), 10);
    }

    [Theory]
    [InlineData(0.5, 1.5)]
    [InlineData(50, 1.01)]
    [InlineData(-10, 3)]
    public void HyperbolicKeplerSolves(double m, double e)
    {
        double h = Conic.SolveHyperbolic(m, e);
        Assert.Equal(m, e * Math.Sinh(h) - h, 8);
    }

    [Fact]
    public void JulianDayOfJ2000()
    {
        Assert.Equal(2451545.0, AstroTime.ToJulianDayUtc(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)), 9);
        var back = AstroTime.FromJulianDayUtc(2451545.0);
        Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), back);
    }
}
