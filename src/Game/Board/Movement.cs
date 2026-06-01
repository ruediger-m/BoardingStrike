using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Pathfinding;

namespace BoardingStrike.Game.Board;

/// <summary>
/// Movement helpers shared by marine execution and enemy AI. A breadth-first
/// flood from a unit's position over walkable hexes (respecting walls, closed
/// doors, and occupancy) yields the cells it could stand on, which both
/// "move toward a destination, up to N" and the AI's destination scoring use.
/// </summary>
public static class Movement
{
    /// <summary>
    /// Path-distance (in steps) from <paramref name="start"/> to every hex
    /// reachable within <paramref name="budget"/> steps. The start hex is
    /// included at distance 0; its own occupancy is ignored, but other occupied
    /// hexes are not traversable.
    /// </summary>
    public static Dictionary<HexCoord, int> ReachableWithin(IHexGraph graph, HexCoord start, int budget)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var dist = new Dictionary<HexCoord, int> { [start] = 0 };
        var queue = new Queue<HexCoord>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            HexCoord current = queue.Dequeue();
            int d = dist[current];
            if (d >= budget)
            {
                continue;
            }

            foreach (HexCoord n in current.Neighbors())
            {
                if (dist.ContainsKey(n) || !graph.Contains(n) || graph.IsBlocked(n) || graph.BlocksMovement(current, n))
                {
                    continue;
                }

                dist[n] = d + 1;
                queue.Enqueue(n);
            }
        }

        return dist;
    }

    /// <summary>
    /// Chooses the reachable hex (within <paramref name="budget"/>) that
    /// minimizes hex-distance to <paramref name="goal"/>, breaking ties by fewer
    /// steps then by coordinate, for determinism. Returns <paramref name="start"/>
    /// if no move gets closer.
    /// </summary>
    public static HexCoord StepToward(IHexGraph graph, HexCoord start, HexCoord goal, int budget)
    {
        Dictionary<HexCoord, int> reachable = ReachableWithin(graph, start, budget);
        return BestCell(reachable, cell => (cell.DistanceTo(goal), reachable[cell]), start);
    }

    /// <summary>
    /// Selects the cell minimizing a (primary, secondary) score, with a final
    /// deterministic coordinate tiebreak. Helper for AI destination scoring.
    /// </summary>
    public static HexCoord BestCell(
        IReadOnlyDictionary<HexCoord, int> candidates,
        Func<HexCoord, (int Primary, int Secondary)> score,
        HexCoord fallback)
    {
        HexCoord best = fallback;
        (int, int, int, int)? bestKey = null;
        foreach (HexCoord cell in candidates.Keys)
        {
            (int primary, int secondary) = score(cell);
            var key = (primary, secondary, cell.Q, cell.R);
            if (bestKey is null || key.CompareTo(bestKey.Value) < 0)
            {
                bestKey = key;
                best = cell;
            }
        }

        return best;
    }
}
