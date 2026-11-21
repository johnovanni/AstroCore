using System.Text.Json;
using AstroCore.Astronomy;
using AstroCore.Bodies;
using AstroCore.Earth;
using AstroCore.Orbits;
using Xunit.Abstractions;

namespace AstroCore.Tests;

/// <summary>
/// Sub-solar and sub-observer points of the Moon, Mars and the Sun, seen from the geocentre, against JPL Horizons
/// (Data/horizons-subpoints.json, written by tools/fetch-horizons-subpoints.ps1). Horizons' values are converted to
/// AstroCore's planetocentric, east-positive convention using the notes Horizons gives for each body.
/// </summary>
public class HorizonsSubPointTests(ITestOutputHelper output)
{
    private const double ToleranceDeg = 0.05;

    private static readonly Func<double, Vector3d> EarthHelio = jd => EarthMoonSystem.Earth.PositionAt(jd);
    private static readonly Func<double, Vector3d> MoonHelio = jd => EarthMoonSystem.Earth.PositionAt(jd) + EarthMoonSystem.Moon.PositionAt(jd);
    private static readonly Func<double, Vector3d> MarsHelio = jd => PlanetOrbits.Mars.PositionAt(jd);
    private static readonly Func<double, Vector3d> SunHelio = _ => new Vector3d(0, 0, 0);

    private sealed record Row(DateTime ObserverUtc, double LightTimeMinutes, GeoPosition? SubObserver, GeoPosition? SubSolar);

    private static readonly Lazy<JsonDocument> Json = new(() =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "horizons-subpoints.json"))));

    /// <summary>Rows for a body, converted to planetocentric latitude and east-positive longitude.</summary>
    private static IReadOnlyList<Row> Rows(string bodyId)
    {
        var body = Json.Value.RootElement.GetProperty("bodies").EnumerateArray()
            .Single(b => b.GetProperty("bodyId").GetString() == bodyId);
        string sense = body.GetProperty("longitudeSense").GetString()!;
        string latType = body.GetProperty("latitudeType").GetString()!;
        Assert.Contains(sense, new[] { "east", "west" });
        Assert.Contains(latType, new[] { "planetodetic", "planetocentric" });
        var (a, b) = BodyConstants.Radii(bodyId);

        GeoPosition? Convert(JsonElement lon, JsonElement lat)
        {
            if (lon.ValueKind == JsonValueKind.Null || lat.ValueKind == JsonValueKind.Null) return null;
            double lonEast = sense == "west" ? -lon.GetDouble() : lon.GetDouble();
            double phi = lat.GetDouble();
            if (latType == "planetodetic")
                phi = Math.Atan(Math.Tan(phi * Frames.Deg) * (b / a) * (b / a)) / Frames.Deg;
            return new GeoPosition(phi, GeoPosition.NormalizeLongitude(lonEast));
        }

        return body.GetProperty("rows").EnumerateArray().Select(r => new Row(
            DateTime.Parse(r.GetProperty("observerUtc").GetString()!, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
            r.GetProperty("lightTimeMinutes").GetDouble(),
            Convert(r.GetProperty("subObserverLonDeg"), r.GetProperty("subObserverLatDeg")),
            Convert(r.GetProperty("subSolarLonDeg"), r.GetProperty("subSolarLatDeg")))).ToList();
    }

    private static double SeparationDeg(GeoPosition p, GeoPosition q)
    {
        var u = p.ToUnitVector();
        var v = q.ToUnitVector();
        return Math.Atan2(Vector3d.Cross(u, v).Length, Vector3d.Dot(u, v)) / Frames.Deg;
    }

    private static string Signed(double x) => (x >= 0 ? "+" : "") + x.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);

    private void AssertAll(string label, IEnumerable<(DateTime utc, GeoPosition expected, GeoPosition actual)> cases)
    {
        double worst = 0;
        var lines = new List<string>();
        foreach (var (utc, expected, actual) in cases)
        {
            double err = SeparationDeg(expected, actual);
            worst = Math.Max(worst, err);
            lines.Add($"{label} {utc:yyyy-MM-dd}: error {err:F4} deg (lat {Signed(actual.LatitudeDeg - expected.LatitudeDeg)}, " +
                      $"lon {Signed(GeoPosition.NormalizeLongitude(actual.LongitudeDeg - expected.LongitudeDeg))}); " +
                      $"Horizons ({expected.LatitudeDeg:F4}, {expected.LongitudeDeg:F4}), AstroCore ({actual.LatitudeDeg:F4}, {actual.LongitudeDeg:F4})");
        }
        Assert.NotEmpty(lines);
        foreach (var l in lines) output.WriteLine(l);
        output.WriteLine($"{label} worst: {worst:F4} deg");
        Assert.True(worst <= ToleranceDeg, $"{label} worst error {worst:F4} deg > {ToleranceDeg} deg:\n" + string.Join("\n", lines));
    }

    /// <summary>
    /// Horizons' sub-solar point is "as seen by the observer at print-time": the body (and its rotation) are taken at
    /// the time the light now arriving left it, so AstroCore's sub-solar point is evaluated at t - one-way light time.
    /// </summary>
    private void CheckSubSolar(string bodyId, Func<double, Vector3d> helio, IBodyRotation rotation) =>
        AssertAll($"{bodyId} sub-solar", Rows(bodyId).Select(r =>
            (r.ObserverUtc, r.SubSolar!.Value,
             BodySubPoints.SubSolarPoint(helio, rotation, r.ObserverUtc.AddMinutes(-r.LightTimeMinutes)))));

    private void CheckSubObserver(string bodyId, Func<double, Vector3d> helio, IBodyRotation rotation) =>
        AssertAll($"{bodyId} sub-observer", Rows(bodyId).Select(r =>
            (r.ObserverUtc, r.SubObserver!.Value,
             BodySubPoints.SubObserverPoint(helio, rotation, EarthHelio, r.ObserverUtc))));

    [Fact] public void MoonSubSolar() => CheckSubSolar("moon", MoonHelio, BodyRotations.Moon);
    [Fact] public void MoonSubObserver() => CheckSubObserver("moon", MoonHelio, BodyRotations.Moon);
    [Fact] public void MarsSubSolar() => CheckSubSolar("mars", MarsHelio, BodyRotations.Mars);
    [Fact] public void MarsSubObserver() => CheckSubObserver("mars", MarsHelio, BodyRotations.Mars);
    [Fact] public void SunSubObserver() => CheckSubObserver("sun", SunHelio, BodyRotations.Sun);
}
