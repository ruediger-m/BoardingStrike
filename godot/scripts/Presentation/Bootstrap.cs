using Godot;
using BoardingStrike.Core;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Scenario;

namespace BoardingStrike.Presentation;

/// <summary>
/// Skeleton bootstrap. Proves the presentation → game → core boundary by
/// loading the committed content, starting the Hangar Sweep scenario, and
/// reporting it on screen and in the Output panel. Replaced by the real
/// ScenarioScene wiring in Step 6 (see docs/plans/iteration-1-mvp.md).
/// </summary>
public partial class Bootstrap : Node2D
{
    public override void _Ready()
    {
        string message;
        try
        {
            string dataDir = ProjectSettings.GlobalizePath("res://data");
            ContentCatalog catalog = ContentCatalog.Load(dataDir);
            ScenarioController scenario = ScenarioController.Start(catalog, "mission_hangar_sweep");
            message = $"{BuildInfo.Banner} — {scenario.MissionName}: "
                + $"{scenario.Marines.Count} marines vs {scenario.Hostiles.Count} hostiles.";
        }
        catch (System.Exception ex)
        {
            message = $"{BuildInfo.Banner} — content load failed: {ex.Message}";
        }

        GD.Print($"[BoardingStrike] {message}");

        var label = GetNodeOrNull<Label>("StatusLabel");
        if (label is not null)
        {
            label.Text = message;
        }
    }
}
