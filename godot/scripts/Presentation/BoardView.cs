using Godot;
using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Scenario;

namespace BoardingStrike.Presentation;

/// <summary>
/// Renders the static board terrain with placeholder programmer art
/// (docs/technical/art-pipeline.md): solid-colour flat-top hexes and wall/door
/// edges. Hex pixel geometry comes from <see cref="HexLayout"/> in Core — the
/// single source of truth shared with the rules engine. Units are drawn
/// separately as <see cref="UnitView"/> nodes so they can animate; call
/// <see cref="Refresh"/> after a door changes to redraw.
/// </summary>
public partial class BoardView : Node2D
{
    /// <summary>Hex circumradius in pixels (center-to-corner).</summary>
    public const float HexSize = 26f;

    private static readonly Color FloorFill = new("2b3038");
    private static readonly Color FloorEdge = new("3a4250");
    private static readonly Color WallColor = new("0d0f12");
    private static readonly Color DoorClosed = new("e5484d"); // warning red
    private static readonly Color DoorOpen = new("5bd6c0");
    private static readonly Color CoordColor = new("8a93a3");

    private readonly HexLayout _layout = new(HexSize);

    private IScenarioView? _view;
    private bool _showCoords;

    /// <summary>Binds the view to a scenario and triggers a redraw.</summary>
    public void Initialize(IScenarioView view)
    {
        _view = view;
        QueueRedraw();
    }

    /// <summary>Re-render (call after the scenario state changes).</summary>
    public void Refresh() => QueueRedraw();

    public void ToggleCoordinates()
    {
        _showCoords = !_showCoords;
        QueueRedraw();
    }

    /// <summary>Pixel center of a hex, for the camera and other nodes.</summary>
    public Vector2 CenterOf(HexCoord hex) => ToVector(_layout.CenterOf(hex));

    public override void _Draw()
    {
        if (_view is null)
        {
            return;
        }

        BoardState board = _view.Board;

        foreach (HexCoord cell in board.FloorCells)
        {
            Vector2[] corners = Corners(cell);
            DrawColoredPolygon(corners, FloorFill);
            DrawPolyline([.. corners, corners[0]], FloorEdge, 1.0f, true);
        }

        foreach (HexEdge edge in board.Walls)
        {
            (Vector2 a, Vector2 b) = EdgePixels(edge);
            DrawLine(a, b, WallColor, 4.0f);
        }

        foreach (HexEdge edge in board.Doors)
        {
            bool open = board.IsDoorOpen(edge.A, edge.B);
            (Vector2 a, Vector2 b) = EdgePixels(edge);
            DrawLine(a, b, open ? DoorOpen : DoorClosed, open ? 2.0f : 5.0f);
        }

        if (_showCoords)
        {
            DrawCoordinates(board);
        }
    }

    private void DrawCoordinates(BoardState board)
    {
        Font font = ThemeDB.FallbackFont;
        foreach (HexCoord cell in board.FloorCells)
        {
            Vector2 c = CenterOf(cell);
            DrawString(font, c - new Vector2(HexSize * 0.5f, 0), $"{cell.Q},{cell.R}", HorizontalAlignment.Center, HexSize, 10, CoordColor);
        }
    }

    private Vector2[] Corners(HexCoord hex)
    {
        Vector2 c = CenterOf(hex);
        var corners = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.DegToRad(60f * i);
            corners[i] = c + new Vector2(Mathf.Cos(angle) * HexSize, Mathf.Sin(angle) * HexSize);
        }

        return corners;
    }

    private (Vector2 A, Vector2 B) EdgePixels(HexEdge edge)
    {
        (PointD first, PointD second) = _layout.SharedEdge(edge.A, edge.B);
        return (ToVector(first), ToVector(second));
    }

    private static Vector2 ToVector(PointD point) => new((float)point.X, (float)point.Y);
}
