# AstroCore

A small shared astronomy library (net10.0, no UI). It was extracted from the SolarSystem3D project (2026-11)
so that several apps can use the same orbit maths; consumers add it as a git submodule.

## Contents

- `AstroCore.Astronomy`: `Vector3d`, `Matrix3d` (including active `RotationX/Y/Z`), `AstroTime`, `Frames`, `Conic` (Kepler solvers).
- `AstroCore.Orbits`: `IOrbitModel`, `PlanetOrbit` (JPL approximate Keplerian elements), `LunarOrbit`
  (ELP-2000/82 principal terms, Meeus ch. 47), `EarthOrbit`, and `EarthMoonSystem` (ready-made barycentre, Moon and Earth).

## Accuracy

Versus JPL Horizons, 2026-11-10:

| Object | Model | Accuracy |
|---|---|---|
| Earth | Earth-Moon barycentre minus the Moon's offset | 0.002 deg |
| Moon | ELP-2000/82 principal terms (Meeus, ch. 47) | 0.002 deg |

## Build and test

```
dotnet test
```
