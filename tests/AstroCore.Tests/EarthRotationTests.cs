using AstroCore.Earth;

namespace AstroCore.Tests;

public class EarthRotationTests
{
    [Fact]
    public void MeeusExample12a() =>
        Assert.Equal(197.693195, EarthRotation.GreenwichMeanSiderealTimeDeg(new DateTime(1987, 4, 10, 0, 0, 0, DateTimeKind.Utc)), 5);

    [Fact]
    public void MeeusExample12b() =>
        Assert.Equal(128.7378734, EarthRotation.GreenwichMeanSiderealTimeDeg(new DateTime(1987, 4, 10, 19, 21, 0, DateTimeKind.Utc)), 5);
}
