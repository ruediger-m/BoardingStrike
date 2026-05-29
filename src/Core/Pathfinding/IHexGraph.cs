using BoardingStrike.Core.Hex;

namespace BoardingStrike.Core.Pathfinding;

/// <summary>
/// The walkability/cost view of a hex map that the pathfinder needs. The
/// Game-layer board implements this (docs/technical/architecture.md): the Core
/// pathfinder stays ignorant of units, doors, terrain types, etc., and only
/// asks these questions.
/// </summary>
public interface IHexGraph
{
    /// <summary>True if the hex is part of the map (in bounds, not void).</summary>
    bool Contains(HexCoord hex);

    /// <summary>
    /// True if the hex cannot be entered (e.g. occupied by another unit or
    /// impassable terrain). The path's start hex is never tested for this.
    /// </summary>
    bool IsBlocked(HexCoord hex);

    /// <summary>
    /// True if the edge between two adjacent hexes blocks movement (a wall, or
    /// a closed door). Called only for adjacent pairs.
    /// </summary>
    bool BlocksMovement(HexCoord from, HexCoord to);

    /// <summary>
    /// The movement cost to enter <paramref name="hex"/>. Should be ≥ 1 so the
    /// hex-distance heuristic remains admissible. Default terrain is 1.
    /// </summary>
    double EnterCost(HexCoord hex);
}
