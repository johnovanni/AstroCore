using AstroCore.Astronomy;

namespace AstroCore.Tests;

public class PrecessionTests
{
    [Fact]
    public void MeeusExample21b()
    {
        var (ra, dec) = Precession.ToEquatorOfDate(41.054063, 49.227750, 2462088.69);
        Assert.Equal(41.547214, ra, 4);
        Assert.Equal(49.348483, dec, 4);
    }

    [Fact]
    public void IdentityAtJ2000()
    {
        var v = Precession.FromJ2000(AstroTime.J2000) * new Vector3d(0.3, -0.5, 0.8);
        Assert.Equal(0.3, v.X, 12); Assert.Equal(-0.5, v.Y, 12); Assert.Equal(0.8, v.Z, 12);
    }
}
