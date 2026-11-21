using AstroCore.Astronomy;
using AstroCore.Earth;

namespace AstroCore.Bodies;

/// <summary>
/// Orientation of a rotating body. Body-fixed frame: +X to (0°, 0°), +Y to (0°, 90°E), +Z to the north pole.
/// </summary>
public interface IBodyRotation
{
    /// <summary>
    /// Converts a direction in the J2000 mean ecliptic frame (any length) into the planetocentric
    /// latitude and east-positive longitude (in (-180, 180]) it points to on the body at <paramref name="utc"/>.
    /// </summary>
    GeoPosition ToBodyFixed(Vector3d directionEclipticJ2000, DateTime utc);
}
