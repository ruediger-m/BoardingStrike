namespace BoardingStrike.Core.Hex;

/// <summary>
/// A hex tile address in axial coordinates (q, r), following the Red Blob Games
/// convention referenced in docs/design/hex-grid.md. Cube coordinates are
/// derived on demand: x = q, z = r, y = -q - r, with x + y + z = 0.
///
/// This is a pure value type with no engine dependency.
/// </summary>
public readonly record struct HexCoord(int Q, int R)
{
    /// <summary>The third cube coordinate (cube y), equal to -Q - R.</summary>
    public int S => -Q - R;

    private static readonly HexCoord[] DirectionOffsets =
    [
        new(1, 0),   // E
        new(1, -1),  // NE
        new(0, -1),  // NW
        new(-1, 0),  // W
        new(-1, 1),  // SW
        new(0, 1),   // SE
    ];

    /// <summary>
    /// The six neighbor offsets, indexed by <see cref="HexDirection"/>.
    /// </summary>
    public static IReadOnlyList<HexCoord> Directions => DirectionOffsets;

    /// <summary>Convenience conversion so callers/tests can write (q, r).</summary>
    public static implicit operator HexCoord((int Q, int R) tuple) => new(tuple.Q, tuple.R);

    public static HexCoord operator +(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R);

    public static HexCoord operator -(HexCoord a, HexCoord b) => new(a.Q - b.Q, a.R - b.R);

    /// <summary>The neighbor one step in the given direction.</summary>
    public HexCoord Neighbor(HexDirection direction) => this + DirectionOffsets[(int)direction];

    /// <summary>The six adjacent hexes, in <see cref="HexDirection"/> order.</summary>
    public IEnumerable<HexCoord> Neighbors()
    {
        foreach (var offset in DirectionOffsets)
        {
            yield return this + offset;
        }
    }

    /// <summary>
    /// Hex distance (number of single steps) to another hex, computed in cube
    /// coordinates: (|dx| + |dy| + |dz|) / 2.
    /// </summary>
    public int DistanceTo(HexCoord other)
    {
        int dq = Q - other.Q;
        int dr = R - other.R;
        int ds = S - other.S;
        return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
    }

    /// <summary>True if <paramref name="other"/> is exactly one step away.</summary>
    public bool IsAdjacentTo(HexCoord other) => DistanceTo(other) == 1;

    public override string ToString() => $"({Q}, {R})";
}
