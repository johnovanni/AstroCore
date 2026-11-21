using AstroCore.Astronomy;
using AstroCore.Earth;

namespace AstroCore.Bodies;

/// <summary>
/// Body orientation from IAU WGCCRE rotational elements. The ICRF (J2000 equatorial) to body-fixed rotation is
/// R3(W) · R1(90° − δ0) · R3(90° + α0), with R1/R3 the frame (passive) rotations of the WGCCRE reports.
/// Time: d and T are counted from J2000 in TT, used as TDB (they differ by under 2 ms).
/// </summary>
public sealed class IauRotationModel(IauRotationElements e) : IBodyRotation
{
    public IauRotationElements Elements { get; } = e;

    /// <summary>α0, δ0 and W in degrees at a TT Julian date.</summary>
    public (double Alpha0Deg, double Delta0Deg, double WDeg) AnglesAt(double jdTT)
    {
        double d = jdTT - AstroTime.J2000;
        double t = d / AstroTime.DaysPerJulianCentury;
        double a = Elements.Alpha0.Evaluate(t), dec = Elements.Delta0.Evaluate(t), w = Elements.W.Evaluate(d);
        foreach (var p in Elements.PeriodicTerms)
        {
            double theta = (p.Phase + (p.RatePerDay + p.RatePerDay2 * d) * d) * Frames.Deg;
            double s = Math.Sin(theta);
            a += p.AlphaAmp * s;
            dec += p.DeltaAmp * Math.Cos(theta);
            w += p.WAmp * s;
        }
        return (a, dec, w);
    }

    public GeoPosition ToBodyFixed(Vector3d directionEclipticJ2000, DateTime utc)
    {
        utc = utc.Kind == DateTimeKind.Local ? utc.ToUniversalTime() : utc;
        var (a, dec, w) = AnglesAt(AstroTime.ToJulianDayTT(utc));
        var eq = Matrix3d.RotationX(Frames.ObliquityJ2000) * directionEclipticJ2000;
        // Matrix3d.RotationZ/X are active rotations, so the passive R3(θ), R1(θ) are RotationZ(−θ), RotationX(−θ).
        var m = Matrix3d.RotationZ(-w * Frames.Deg)
              * Matrix3d.RotationX(-(90.0 - dec) * Frames.Deg)
              * Matrix3d.RotationZ(-(90.0 + a) * Frames.Deg);
        return GeoPosition.FromVector(m * eq);
    }
}
