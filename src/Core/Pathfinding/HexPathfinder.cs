using BoardingStrike.Core.Hex;

namespace BoardingStrike.Core.Pathfinding;

/// <summary>
/// A* shortest-path search over a hex grid (docs/design/hex-grid.md). Movement
/// is per-edge: a step is allowed only between adjacent, in-map, non-blocked
/// hexes whose shared edge does not block movement.
///
/// <para>
/// The search is <b>deterministic</b>: the open-set priority breaks ties first
/// by lower heuristic, then by insertion order, so the same inputs always yield
/// the same path. This matters for reproducible AI turns and tests
/// (docs/technical/architecture.md "Determinism").
/// </para>
///
/// This is a candidate for a future C++/GDExtension port; the C# implementation
/// keeps a clean interface boundary (<see cref="IHexGraph"/>) so a port stays
/// surgical.
/// </summary>
public static class HexPathfinder
{
    /// <summary>
    /// Finds a least-cost path from <paramref name="start"/> to
    /// <paramref name="goal"/>, or returns <c>null</c> if none exists.
    /// </summary>
    public static HexPath? FindPath(IHexGraph graph, HexCoord start, HexCoord goal)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (!graph.Contains(start) || !graph.Contains(goal))
        {
            return null;
        }

        if (graph.IsBlocked(goal))
        {
            return null;
        }

        if (start == goal)
        {
            return new HexPath([start], 0.0);
        }

        var cameFrom = new Dictionary<HexCoord, HexCoord>();
        var gScore = new Dictionary<HexCoord, double> { [start] = 0.0 };
        var closed = new HashSet<HexCoord>();

        // Priority: (fScore, heuristic, insertion order) — fully deterministic.
        var open = new PriorityQueue<HexCoord, (double F, int H, long Seq)>();
        long sequence = 0;
        open.Enqueue(start, (Heuristic(start, goal), start.DistanceTo(goal), sequence++));

        while (open.Count > 0)
        {
            HexCoord current = open.Dequeue();

            if (current == goal)
            {
                return Reconstruct(cameFrom, gScore, goal);
            }

            // Skip stale queue entries (a better path to this hex was already processed).
            if (!closed.Add(current))
            {
                continue;
            }

            double currentG = gScore[current];

            foreach (HexCoord neighbor in current.Neighbors())
            {
                if (!graph.Contains(neighbor) || graph.IsBlocked(neighbor))
                {
                    continue;
                }

                if (graph.BlocksMovement(current, neighbor))
                {
                    continue;
                }

                double tentativeG = currentG + graph.EnterCost(neighbor);

                if (!gScore.TryGetValue(neighbor, out double knownG) || tentativeG < knownG - 1e-9)
                {
                    gScore[neighbor] = tentativeG;
                    cameFrom[neighbor] = current;
                    open.Enqueue(neighbor, (tentativeG + Heuristic(neighbor, goal), neighbor.DistanceTo(goal), sequence++));
                }
            }
        }

        return null;
    }

    private static double Heuristic(HexCoord from, HexCoord to) => from.DistanceTo(to);

    private static HexPath Reconstruct(
        IReadOnlyDictionary<HexCoord, HexCoord> cameFrom,
        IReadOnlyDictionary<HexCoord, double> gScore,
        HexCoord goal)
    {
        var reversed = new List<HexCoord> { goal };
        HexCoord node = goal;
        while (cameFrom.TryGetValue(node, out HexCoord previous))
        {
            node = previous;
            reversed.Add(node);
        }

        reversed.Reverse();
        return new HexPath(reversed, gScore[goal]);
    }
}
