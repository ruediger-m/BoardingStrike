using BoardingStrike.Core.Hex;

namespace BoardingStrike.Core.Pathfinding;

/// <summary>
/// A found path: the ordered hexes from start to goal (inclusive of both) and
/// the total movement cost to traverse it (sum of entered-hex costs; the start
/// hex contributes nothing).
/// </summary>
public sealed record HexPath(IReadOnlyList<HexCoord> Hexes, double Cost)
{
    /// <summary>Number of steps (entered hexes) = Hexes.Count - 1.</summary>
    public int StepCount => Hexes.Count - 1;
}
