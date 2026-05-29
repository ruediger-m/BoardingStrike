using BoardingStrike.Core.Hex;
using Xunit;

namespace BoardingStrike.Game.Tests.Core;

public sealed class HexLayoutTests
{
    private static readonly HexLayout Layout = HexLayout.Unit;
    private const double Tolerance = 1e-9;

    [Fact]
    public void All_six_neighbor_centers_are_root_three_apart()
    {
        var origin = new HexCoord(0, 0);
        PointD center = Layout.CenterOf(origin);
        double expected = Math.Sqrt(3.0) * Layout.Size;

        foreach (HexCoord neighbor in origin.Neighbors())
        {
            double distance = (Layout.CenterOf(neighbor) - center).Length;
            Assert.Equal(expected, distance, Tolerance);
        }
    }

    [Fact]
    public void Shared_edge_length_equals_side_length()
    {
        var a = new HexCoord(3, 1);
        foreach (HexCoord b in a.Neighbors())
        {
            (PointD e1, PointD e2) = Layout.SharedEdge(a, b);
            double length = (e2 - e1).Length;
            Assert.Equal(Layout.Size, length, Tolerance);
        }
    }

    [Fact]
    public void Shared_edge_is_centered_on_and_perpendicular_to_the_center_join()
    {
        var a = new HexCoord(0, 0);
        var b = new HexCoord(1, 0);

        PointD centerJoinMid = (Layout.CenterOf(a) + Layout.CenterOf(b)) * 0.5;
        (PointD e1, PointD e2) = Layout.SharedEdge(a, b);
        PointD edgeMid = (e1 + e2) * 0.5;

        Assert.Equal(centerJoinMid.X, edgeMid.X, Tolerance);
        Assert.Equal(centerJoinMid.Y, edgeMid.Y, Tolerance);

        // Edge direction perpendicular to center-join direction => dot product ~ 0.
        PointD join = Layout.CenterOf(b) - Layout.CenterOf(a);
        PointD edge = e2 - e1;
        double dot = (join.X * edge.X) + (join.Y * edge.Y);
        Assert.Equal(0.0, dot, Tolerance);
    }
}
