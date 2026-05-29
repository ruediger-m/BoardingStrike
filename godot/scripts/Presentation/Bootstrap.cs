using Godot;
using BoardingStrike.Game.Scenario;

namespace BoardingStrike.Presentation;

/// <summary>
/// Skeleton bootstrap (Iteration 1, Step 1). Its only job is to prove the
/// dependency boundary: the Godot presentation layer reaches the engine-
/// independent Game layer (which in turn reaches Core), with no Godot type
/// leaking the other way.
///
/// Replaced by the real ScenarioScene wiring in Step 6 (see
/// docs/plans/iteration-1-mvp.md and docs/technical/architecture.md).
/// </summary>
public partial class Bootstrap : Node2D
{
    public override void _Ready()
    {
        var controller = new ScenarioController();
        string message = controller.Ping();

        // Sign of life in the editor/console output.
        GD.Print($"[BoardingStrike] {message}");

        // And on screen, so opening the project shows something immediately.
        var label = GetNodeOrNull<Label>("StatusLabel");
        if (label is not null)
        {
            label.Text = message;
        }
    }
}
