# AstroCore

A small shared astronomy library (net10.0, no UI). It was extracted from the SolarSystem3D project (2026-11)
so that several apps can use the same orbit maths; consumers add it as a git submodule.

## Contents

- `AstroCore.Astronomy`: `Vector3d`, `Matrix3d` (including active `RotationX/Y/Z`), `AstroTime`, `Frames`, `Conic` (Kepler solvers).
- `AstroCore.Earth`: `EarthRotation`, `GeoPosition`, `SubPoints` (Sun/Moon RA-Dec, sub-solar/sub-lunar points, Moon phase), `Terminator`.
- `AstroCore.Bodies`: `IBodyRotation`, `EarthRotationModel`, `IauRotationModel` and `IauRotationElements`, `BodyRotations`, `BodyConstants`, `BodySubPoints`.
- `AstroCore.Orbits`: `IOrbitModel`, `PlanetOrbit` (JPL approximate Keplerian elements), `PlanetOrbits.Mars`, `LunarOrbit`
  (ELP-2000/82 principal terms, Meeus ch. 47), `EarthOrbit`, and `EarthMoonSystem` (ready-made barycentre, Moon and Earth).

## Accuracy

Versus JPL Horizons, 2026-11-10:

| Object | Model | Accuracy |
|---|---|---|
| Earth | Earth-Moon barycentre minus the Moon's offset | 0.002 deg |
| Moon | ELP-2000/82 principal terms (Meeus, ch. 47) | 0.002 deg |
| Sub-solar / sub-lunar point (vs Horizons apparent RA/Dec, 5 dates 2026–2030) | Sun and Moon RA/Dec of date, GMST | Sun worst 0.0092 deg, Moon worst 0.0120 deg (limit 0.05) |

## Bodies

`AstroCore.Bodies` gives body orientation and the sub-solar / sub-observer points of any body
(`BodySubPoints`, with light time for the sub-observer point). Longitudes are planetocentric and east-positive.

| Body | Rotation (`BodyRotations`) | Source of the elements | Radii (`BodyConstants`, km) |
|---|---|---|---|
| Earth | `EarthRotationModel` (precession + GMST, same as `SubPoints`) | — | 6378.1366 / 6356.7519 |
| Sun | `IauRotationModel` | IAU WGCCRE 2015 report (Archinal et al. 2018, doi:10.1007/s10569-017-9805-5), Table 1 | 695700 (sphere) |
| Moon | `IauRotationModel`, with the E1–E13 terms | IAU WGCCRE 2009 report (Archinal et al. 2011, doi:10.1007/s10569-010-9320-4), as carried by NAIF `pck00011.tpc`; the 2015 report dropped this series | 1737.4 (sphere) |
| Mars | `IauRotationModel`, with its periodic terms | IAU WGCCRE 2015 report, Table 1 | 3396.19 / 3376.20 |

Radii are from the 2015 report, Tables 4 and 5. Mars' orbit is `PlanetOrbits.Mars` (JPL approximate elements).

Versus JPL Horizons (geocentric observer, 6 dates 2000–2035, `tests/AstroCore.Tests/Data/horizons-subpoints.json`,
written by `tools/fetch-horizons-subpoints.ps1`; limit 0.05 deg, great-circle):

| Point | Worst error |
|---|---|
| Moon sub-solar | 0.0078 deg |
| Moon sub-observer | 0.0117 deg (Horizons uses the DE441 MOON_ME frame) |
| Mars sub-solar | 0.0104 deg |
| Mars sub-observer | 0.0293 deg (2035-09-01, Mars at 0.39 AU: orbit-model error is magnified near opposition) |
| Sun sub-observer | 0.0043 deg |

## Build and test

```
dotnet test
```
