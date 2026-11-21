namespace AstroCore.Bodies;

/// <summary>Quadratic polynomial C0 + C1·x + C2·x², in degrees.</summary>
public sealed record Polynomial(double C0, double C1, double C2)
{
    public double Evaluate(double x) => C0 + (C1 + C2 * x) * x;
}

/// <summary>
/// One periodic term of a WGCCRE rotation model. Its argument is θ = Phase + RatePerDay·d + RatePerDay2·d² (degrees,
/// d in days from J2000 TDB). It adds AlphaAmp·sin θ to α0, DeltaAmp·cos θ to δ0 and WAmp·sin θ to W (degrees).
/// </summary>
public sealed record PeriodicTerm(double AlphaAmp, double DeltaAmp, double WAmp, double Phase, double RatePerDay, double RatePerDay2);

/// <summary>
/// IAU WGCCRE rotational elements: north-pole right ascension α0 and declination δ0 (ICRF, degrees; polynomials in T,
/// Julian centuries from J2000 TDB) and prime-meridian angle W (degrees; polynomial in d, days from J2000 TDB),
/// plus periodic terms.
/// </summary>
public sealed record IauRotationElements(Polynomial Alpha0, Polynomial Delta0, Polynomial W, IReadOnlyList<PeriodicTerm> PeriodicTerms)
{
    private const double DaysPerCentury = 36525.0;

    /// <summary>
    /// The Sun. Archinal et al. 2018, "Report of the IAU Working Group on Cartographic Coordinates and Rotational
    /// Elements: 2015", Celest. Mech. Dyn. Astr. 130:22, doi:10.1007/s10569-017-9805-5, Table 1:
    /// α0 = 286.13, δ0 = 63.87, W = 84.176 + 14.1844000d. (Same values in NAIF pck00011.tpc, BODY10_*.)
    /// </summary>
    public static IauRotationElements Sun { get; } = new(
        new Polynomial(286.13, 0, 0),
        new Polynomial(63.87, 0, 0),
        new Polynomial(84.176, 14.1844000, 0),
        []);

    /// <summary>
    /// The Moon (mean-Earth/polar-axis frame, low-precision series). The 2015 report (Archinal et al. 2018, Table 2:
    /// "Moon — See Sect. 3") removed this low-precision series (abstract of that report), so the values are those
    /// of the 2009 report: Archinal et al. 2011, Celest. Mech. Dyn. Astr. 109:101–135, doi:10.1007/s10569-010-9320-4
    /// (not open access), as carried by NAIF pck00011.tpc (BODY301_POLE_RA/DEC/PM, BODY301_NUT_PREC_RA/DEC/PM, and the E1–E13
    /// rates in degrees per day from the comment above BODY3_NUT_PREC_ANGLES):
    /// α0 = 269.9949 + 0.0031T − 3.8787 sin E1 − 0.1204 sin E2 + 0.0700 sin E3 − 0.0172 sin E4 + 0.0072 sin E6
    ///      − 0.0052 sin E10 + 0.0043 sin E13;
    /// δ0 = 66.5392 + 0.0130T + 1.5419 cos E1 + 0.0239 cos E2 − 0.0278 cos E3 + 0.0068 cos E4 − 0.0029 cos E6
    ///      + 0.0009 cos E7 + 0.0008 cos E10 − 0.0009 cos E13;
    /// W  = 38.3213 + 13.17635815d − 1.4e−12 d² + 3.5610 sin E1 + 0.1208 sin E2 − 0.0642 sin E3 + 0.0158 sin E4
    ///      + 0.0252 sin E5 − 0.0066 sin E6 − 0.0047 sin E7 − 0.0046 sin E8 + 0.0028 sin E9 + 0.0052 sin E10
    ///      + 0.0040 sin E11 + 0.0019 sin E12 − 0.0044 sin E13.
    /// </summary>
    public static IauRotationElements Moon { get; } = new(
        new Polynomial(269.9949, 0.0031, 0),
        new Polynomial(66.5392, 0.0130, 0),
        new Polynomial(38.3213, 13.17635815, -1.4e-12),
        [
            //               α amp    δ amp    W amp    phase     rate (deg/day)
            new PeriodicTerm(-3.8787, 1.5419, 3.5610, 125.045, -0.0529921, 0), // E1
            new PeriodicTerm(-0.1204, 0.0239, 0.1208, 250.089, -0.1059842, 0), // E2
            new PeriodicTerm(0.0700, -0.0278, -0.0642, 260.008, 13.0120009, 0), // E3
            new PeriodicTerm(-0.0172, 0.0068, 0.0158, 176.625, 13.3407154, 0), // E4
            new PeriodicTerm(0.0, 0.0, 0.0252, 357.529, 0.9856003, 0),          // E5
            new PeriodicTerm(0.0072, -0.0029, -0.0066, 311.589, 26.4057084, 0), // E6
            new PeriodicTerm(0.0, 0.0009, -0.0047, 134.963, 13.0649930, 0),     // E7
            new PeriodicTerm(0.0, 0.0, -0.0046, 276.617, 0.3287146, 0),         // E8
            new PeriodicTerm(0.0, 0.0, 0.0028, 34.226, 1.7484877, 0),           // E9
            new PeriodicTerm(-0.0052, 0.0008, 0.0052, 15.134, -0.1589763, 0),   // E10
            new PeriodicTerm(0.0, 0.0, 0.0040, 119.743, 0.0036096, 0),          // E11
            new PeriodicTerm(0.0, 0.0, 0.0019, 239.961, 0.1643573, 0),          // E12
            new PeriodicTerm(0.0043, -0.0009, -0.0044, 25.053, 12.9590088, 0),  // E13
        ]);

    /// <summary>
    /// Mars. Archinal et al. 2018 (2015 report), doi:10.1007/s10569-017-9805-5, Table 1 (also NAIF pck00011.tpc,
    /// BODY499_* and BODY4_NUT_PREC_ANGLES 11–26). Angle rates are per Julian century T in the table; they are kept
    /// as printed and divided by 36525 here.
    /// α0 = 317.269202 − 0.10927547T + 0.000068 sin(198.991226 + 19139.4819985T) + 0.000238 sin(226.292679 + 38280.8511281T)
    ///      + 0.000052 sin(249.663391 + 57420.7251593T) + 0.000009 sin(266.183510 + 76560.6367950T)
    ///      + 0.419057 sin(79.398797 + 0.5042615T);
    /// δ0 = 54.432516 − 0.05827105T + 0.000051 cos(122.433576 + 19139.9407476T) + 0.000141 cos(43.058401 + 38280.8753272T)
    ///      + 0.000031 cos(57.663379 + 57420.7517205T) + 0.000005 cos(79.476401 + 76560.6495004T)
    ///      + 1.591274 cos(166.325722 + 0.5042615T);
    /// W  = 176.049863 + 350.891982443297d + 0.000145 sin(129.071773 + 19140.0328244T) + 0.000157 sin(36.352167 + 38281.0473591T)
    ///      + 0.000040 sin(56.668646 + 57420.9295360T) + 0.000001 sin(67.364003 + 76560.2552215T)
    ///      + 0.000001 sin(104.792680 + 95700.4387578T) + 0.584542 sin(95.391654 + 0.5042615T).
    /// </summary>
    public static IauRotationElements Mars { get; } = new(
        new Polynomial(317.269202, -0.10927547, 0),
        new Polynomial(54.432516, -0.05827105, 0),
        new Polynomial(176.049863, 350.891982443297, 0),
        [
            // α0 terms (sin)
            new PeriodicTerm(0.000068, 0, 0, 198.991226, 19139.4819985 / DaysPerCentury, 0),
            new PeriodicTerm(0.000238, 0, 0, 226.292679, 38280.8511281 / DaysPerCentury, 0),
            new PeriodicTerm(0.000052, 0, 0, 249.663391, 57420.7251593 / DaysPerCentury, 0),
            new PeriodicTerm(0.000009, 0, 0, 266.183510, 76560.6367950 / DaysPerCentury, 0),
            new PeriodicTerm(0.419057, 0, 0, 79.398797, 0.5042615 / DaysPerCentury, 0),
            // δ0 terms (cos)
            new PeriodicTerm(0, 0.000051, 0, 122.433576, 19139.9407476 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0.000141, 0, 43.058401, 38280.8753272 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0.000031, 0, 57.663379, 57420.7517205 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0.000005, 0, 79.476401, 76560.6495004 / DaysPerCentury, 0),
            new PeriodicTerm(0, 1.591274, 0, 166.325722, 0.5042615 / DaysPerCentury, 0),
            // W terms (sin)
            new PeriodicTerm(0, 0, 0.000145, 129.071773, 19140.0328244 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0, 0.000157, 36.352167, 38281.0473591 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0, 0.000040, 56.668646, 57420.9295360 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0, 0.000001, 67.364003, 76560.2552215 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0, 0.000001, 104.792680, 95700.4387578 / DaysPerCentury, 0),
            new PeriodicTerm(0, 0, 0.584542, 95.391654, 0.5042615 / DaysPerCentury, 0),
        ]);
}
