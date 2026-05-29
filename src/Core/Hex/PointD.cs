namespace BoardingStrike.Core.Hex;

/// <summary>
/// A minimal double-precision 2D point/vector used for hex layout and the
/// line-of-sight geometry. Double precision (rather than float) keeps the
/// edge-crossing math robust. No engine dependency by design.
/// </summary>
public readonly record struct PointD(double X, double Y)
{
    public static PointD operator +(PointD a, PointD b) => new(a.X + b.X, a.Y + b.Y);

    public static PointD operator -(PointD a, PointD b) => new(a.X - b.X, a.Y - b.Y);

    public static PointD operator *(PointD a, double scalar) => new(a.X * scalar, a.Y * scalar);

    /// <summary>Euclidean length.</summary>
    public double Length => Math.Sqrt((X * X) + (Y * Y));

    /// <summary>Unit vector in the same direction; returns the zero vector unchanged.</summary>
    public PointD Normalized()
    {
        double length = Length;
        return length <= double.Epsilon ? this : new PointD(X / length, Y / length);
    }

    /// <summary>The left-hand perpendicular (rotate 90°).</summary>
    public PointD Perp() => new(-Y, X);

    /// <summary>2D cross product (z-component), useful for segment intersection.</summary>
    public static double Cross(PointD a, PointD b) => (a.X * b.Y) - (a.Y * b.X);
}
