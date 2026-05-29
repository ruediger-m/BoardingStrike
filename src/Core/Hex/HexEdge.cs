namespace BoardingStrike.Core.Hex;

/// <summary>
/// An undirected edge between two adjacent hexes. Edges are the home of walls
/// and doors (docs/design/hex-grid.md): blockers live on the boundary between
/// two hexes, not on either hex.
///
/// The endpoints are stored in a canonical order so that {a, b} and {b, a}
/// compare equal and hash identically — making this a safe dictionary/set key.
/// </summary>
public readonly struct HexEdge : IEquatable<HexEdge>
{
    public HexCoord A { get; }

    public HexCoord B { get; }

    /// <summary>
    /// Creates an edge between two adjacent hexes. Order is normalized.
    /// </summary>
    /// <exception cref="ArgumentException">If the hexes are not adjacent.</exception>
    public HexEdge(HexCoord a, HexCoord b)
    {
        if (!a.IsAdjacentTo(b))
        {
            throw new ArgumentException($"Hexes {a} and {b} are not adjacent; an edge requires adjacency.");
        }

        // Canonical ordering: by Q then R, so the edge is direction-agnostic.
        if (a.Q < b.Q || (a.Q == b.Q && a.R <= b.R))
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }

    public bool Equals(HexEdge other) => A.Equals(other.A) && B.Equals(other.B);

    public override bool Equals(object? obj) => obj is HexEdge other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(A, B);

    public static bool operator ==(HexEdge left, HexEdge right) => left.Equals(right);

    public static bool operator !=(HexEdge left, HexEdge right) => !left.Equals(right);

    public override string ToString() => $"[{A} | {B}]";
}
