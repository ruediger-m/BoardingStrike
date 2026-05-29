using BoardingStrike.Core.Hex;

namespace BoardingStrike.Core.Fov;

/// <summary>
/// Line-of-sight over a flat-top hex grid using the edge-line algorithm from
/// docs/design/line-of-sight-and-doors.md: cast the straight segment between
/// two hex centers and block sight if it crosses any wall or closed-door edge.
///
/// <para>
/// The "you can't see around a corner" conservative rule is implemented by
/// casting <b>two</b> rays nudged a hair to either side of the true center
/// line. Each nudged ray avoids passing exactly through a vertex, so its
/// hex-to-hex march is unambiguous (exactly one exit edge per step). When the
/// true line grazes a corner, the two nudged rays pass on opposite sides of it;
/// requiring <i>both</i> to be unobstructed yields the conservative result.
/// </para>
///
/// Sight is symmetric. Units never block sight — only edges do. This is a
/// candidate for a future C++/GDExtension port.
/// </summary>
public static class LineOfSight
{
    private static readonly HexLayout Layout = HexLayout.Unit;

    // A nudge small relative to the hex size: large enough to dodge exact
    // vertices, small enough never to change which hexes a ray passes through
    // (other than at a grazed corner).
    private const double Nudge = 1e-3;

    // Tolerance for parameter range tests in the segment intersection.
    private const double Eps = 1e-9;

    /// <summary>
    /// True if <paramref name="from"/> has line of sight to <paramref name="to"/>.
    /// </summary>
    public static bool HasLineOfSight(ISightMap map, HexCoord from, HexCoord to)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (from == to)
        {
            return map.Contains(from);
        }

        if (!map.Contains(from) || !map.Contains(to))
        {
            return false;
        }

        PointD centerFrom = Layout.CenterOf(from);
        PointD centerTo = Layout.CenterOf(to);
        PointD offset = (centerTo - centerFrom).Normalized().Perp() * Nudge;

        // Conservative AND of the two straddling rays.
        return RayIsClear(map, from, to, centerFrom + offset, centerTo + offset)
            && RayIsClear(map, from, to, centerFrom - offset, centerTo - offset);
    }

    /// <summary>
    /// Marches a single ray hex-by-hex from <paramref name="start"/> toward
    /// <paramref name="goal"/>, returning false if it crosses a sight-blocking
    /// edge or wanders off the map before arriving.
    /// </summary>
    private static bool RayIsClear(ISightMap map, HexCoord start, HexCoord goal, PointD rayA, PointD rayB)
    {
        HexCoord current = start;
        double currentT = 0.0;
        int maxSteps = (start.DistanceTo(goal) * 2) + 8; // generous guard against numeric edge cases

        for (int step = 0; step < maxSteps; step++)
        {
            if (current == goal)
            {
                return true;
            }

            // Find the edge this ray exits through: the forward edge crossing
            // with the smallest parameter t greater than where we are now.
            double bestT = double.PositiveInfinity;
            HexCoord exitNeighbor = default;
            bool found = false;

            foreach (HexCoord neighbor in current.Neighbors())
            {
                (PointD e1, PointD e2) = Layout.SharedEdge(current, neighbor);
                if (!TryIntersect(rayA, rayB, e1, e2, out double t, out double u))
                {
                    continue;
                }

                bool forward = t > currentT + Eps && t <= 1.0 + Eps;
                bool onEdge = u > -Eps && u < 1.0 + Eps;
                if (forward && onEdge && t < bestT)
                {
                    bestT = t;
                    exitNeighbor = neighbor;
                    found = true;
                }
            }

            if (!found)
            {
                // The ray ended (reached rayB) without arriving at goal, or a
                // numeric anomaly occurred. Treat as no sight, conservatively.
                return false;
            }

            if (!map.Contains(exitNeighbor) || map.BlocksSight(current, exitNeighbor))
            {
                return false;
            }

            current = exitNeighbor;
            currentT = bestT;
        }

        return current == goal;
    }

    /// <summary>
    /// Intersects ray segment P→P2 with edge segment Q→Q2. Outputs
    /// <paramref name="t"/> along the ray and <paramref name="u"/> along the
    /// edge. Returns false if the segments are parallel.
    /// </summary>
    private static bool TryIntersect(PointD p, PointD p2, PointD q, PointD q2, out double t, out double u)
    {
        t = 0.0;
        u = 0.0;

        PointD r = p2 - p;
        PointD s = q2 - q;
        double denominator = PointD.Cross(r, s);
        if (Math.Abs(denominator) < 1e-12)
        {
            return false;
        }

        PointD qp = q - p;
        t = PointD.Cross(qp, s) / denominator;
        u = PointD.Cross(qp, r) / denominator;
        return true;
    }
}
