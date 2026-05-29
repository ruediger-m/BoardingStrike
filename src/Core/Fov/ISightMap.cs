using BoardingStrike.Core.Hex;

namespace BoardingStrike.Core.Fov;

/// <summary>
/// The sight-blocking view of a hex map that the line-of-sight query needs.
/// Kept separate from movement (<see cref="Pathfinding.IHexGraph"/>) because a
/// Game board answers the two questions differently even though a closed door
/// blocks both. A board may implement both interfaces.
/// </summary>
public interface ISightMap
{
    /// <summary>True if the hex is part of the map (in bounds, not void).</summary>
    bool Contains(HexCoord hex);

    /// <summary>
    /// True if the edge between two adjacent hexes blocks line of sight (a wall
    /// or a closed door). Called only for adjacent pairs. Units do not block
    /// sight (docs/design/line-of-sight-and-doors.md).
    /// </summary>
    bool BlocksSight(HexCoord a, HexCoord b);
}
