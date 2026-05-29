namespace BoardingStrike.Core.Hex;

/// <summary>
/// Converts axial hex coordinates to world-space points for a <b>flat-top</b>
/// hex grid (docs/design/hex-grid.md). This is the single source of truth for
/// hex pixel geometry — the renderer's constants (docs/technical/art-pipeline.md
/// "HexRender") and the line-of-sight math both derive from here.
///
/// <para>
/// For a flat-top hex with circumradius (center-to-corner) = <see cref="Size"/>:
/// adjacent centers are <c>sqrt(3) * Size</c> apart, the side length equals
/// <see cref="Size"/>, and the apothem (center to edge midpoint) is
/// <c>sqrt(3)/2 * Size</c>.
/// </para>
/// </summary>
public sealed class HexLayout
{
    private static readonly double Sqrt3 = Math.Sqrt(3.0);

    /// <summary>A unit layout (Size = 1). Geometry is scale-invariant for LoS.</summary>
    public static HexLayout Unit { get; } = new(1.0);

    public double Size { get; }

    public HexLayout(double size = 1.0)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Hex size must be positive.");
        }

        Size = size;
    }

    /// <summary>World-space center of a hex (flat-top axial → pixel).</summary>
    public PointD CenterOf(HexCoord hex) =>
        new(Size * 1.5 * hex.Q, Size * Sqrt3 * (hex.R + (hex.Q / 2.0)));

    /// <summary>
    /// The two endpoints of the edge shared by two adjacent hexes, in world
    /// space. Derived generically from the two centers (the shared edge is the
    /// segment centered at their midpoint, perpendicular to the center-join,
    /// with length equal to the hex side). This avoids per-direction corner
    /// bookkeeping and works for all six neighbors.
    /// </summary>
    /// <exception cref="ArgumentException">If the hexes are not adjacent.</exception>
    public (PointD First, PointD Second) SharedEdge(HexCoord a, HexCoord b)
    {
        if (!a.IsAdjacentTo(b))
        {
            throw new ArgumentException($"Hexes {a} and {b} are not adjacent; no shared edge.");
        }

        PointD centerA = CenterOf(a);
        PointD centerB = CenterOf(b);
        PointD midpoint = (centerA + centerB) * 0.5;
        PointD perpendicular = (centerB - centerA).Normalized().Perp();
        double halfSide = Size * 0.5;
        return (midpoint + (perpendicular * halfSide), midpoint - (perpendicular * halfSide));
    }
}
