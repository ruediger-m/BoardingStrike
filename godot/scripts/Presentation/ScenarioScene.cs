using Godot;
using BoardingStrike.Core;
using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Scenario;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Presentation;

/// <summary>
/// Root of the scenario scene. Loads the committed content, starts the Hangar
/// Sweep mission, hands the board to <see cref="BoardView"/>, and frames the
/// camera. Step 6: a static, rendered view. Player input and turn playback
/// arrive in Steps 7–8.
///
/// <para>Press <b>F1</b> to toggle the hex-coordinate debug overlay.</para>
/// </summary>
public partial class ScenarioScene : Node2D
{
    private BoardView _boardView = null!;
    private Camera2D _camera = null!;
    private Label _statusLabel = null!;

    public override void _Ready()
    {
        _boardView = GetNode<BoardView>("BoardView");
        _camera = GetNode<Camera2D>("Camera2D");
        _statusLabel = GetNode<Label>("Hud/StatusLabel");

        try
        {
            string dataDir = ProjectSettings.GlobalizePath("res://data");
            ContentCatalog catalog = ContentCatalog.Load(dataDir);
            ScenarioController scenario = ScenarioController.Start(catalog, "mission_hangar_sweep");

            _boardView.Initialize(scenario);
            FrameCamera(scenario);

            _statusLabel.Text =
                $"{BuildInfo.Banner} — {scenario.MissionName}\n"
                + $"{scenario.Marines.Count} marines vs {scenario.Hostiles.Count} hostiles   (F1: coordinates)";
        }
        catch (System.Exception ex)
        {
            _statusLabel.Text = $"{BuildInfo.Banner} — content load failed: {ex.Message}";
            GD.PrintErr(ex);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.F1 })
        {
            _boardView.ToggleCoordinates();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>Centers and zooms the camera so the whole map is visible with a margin.</summary>
    private void FrameCamera(IScenarioView scenario)
    {
        bool any = false;
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (HexCoord cell in scenario.Board.FloorCells)
        {
            Vector2 c = _boardView.CenterOf(cell);
            min = new Vector2(Mathf.Min(min.X, c.X), Mathf.Min(min.Y, c.Y));
            max = new Vector2(Mathf.Max(max.X, c.X), Mathf.Max(max.Y, c.Y));
            any = true;
        }

        if (!any)
        {
            return;
        }

        float margin = BoardView.HexSize * 2f;
        Vector2 content = (max - min) + new Vector2(margin * 2, margin * 2);
        Vector2 viewport = GetViewportRect().Size;
        float zoom = Mathf.Clamp(Mathf.Min(viewport.X / content.X, viewport.Y / content.Y), 0.2f, 1.5f);

        _camera.Position = (min + max) * 0.5f;
        _camera.Zoom = new Vector2(zoom, zoom);
        _camera.MakeCurrent();
    }
}
