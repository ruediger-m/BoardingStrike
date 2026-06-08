using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using BoardingStrike.Core;
using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Events;
using BoardingStrike.Game.Scenario;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Presentation;

/// <summary>
/// Root of the scenario scene. Loads content, starts the Hangar Sweep mission,
/// then auto-plays it round by round (marine decisions come from
/// <see cref="AutoMarineController"/>; the real click-to-play UI is Step 8) and
/// animates the rules-engine event stream: movement tweens, attack flashes, HP
/// bars, conditions, and doors. State has already moved in the engine; this is
/// purely the view replaying what happened.
///
/// <para>Controls: <b>F1</b> coordinates · <b>Space</b> pause/resume · <b>R</b> restart.</para>
/// </summary>
public partial class ScenarioScene : Node2D
{
    private const double MoveDuration = 0.14;
    private const double StepPause = 0.05;
    private const double RoundGap = 0.5;

    private static readonly Color AttackerFlash = new("ffffff");
    private static readonly Color HitFlash = new("e5484d");
    private static readonly Color HealFlash = new("7fd44a");

    private readonly Dictionary<string, UnitView> _units = [];
    private readonly List<string> _log = [];
    private readonly AutoMarineController _controller = new();

    private BoardView _boardView = null!;
    private Camera2D _camera = null!;
    private Label _statusLabel = null!;
    private Label _logLabel = null!;
    private Node2D _unitsRoot = null!;
    private ScenarioController _scenario = null!;
    private bool _paused;

    public override void _Ready()
    {
        _boardView = GetNode<BoardView>("BoardView");
        _camera = GetNode<Camera2D>("Camera2D");
        _statusLabel = GetNode<Label>("Hud/StatusLabel");
        _logLabel = CreateLogLabel();

        try
        {
            string dataDir = ProjectSettings.GlobalizePath("res://data");
            ContentCatalog catalog = ContentCatalog.Load(dataDir);
            _scenario = ScenarioController.Start(catalog, "mission_hangar_sweep");

            _boardView.Initialize(_scenario);
            SpawnUnits();
            FrameCamera();
            UpdateStatus();

            _ = RunAsync();
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"{BuildInfo.Banner} — content load failed: {ex.Message}";
            GD.PrintErr(ex);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true } key)
        {
            return;
        }

        switch (key.Keycode)
        {
            case Key.F1:
                _boardView.ToggleCoordinates();
                GetViewport().SetInputAsHandled();
                break;
            case Key.Space:
                _paused = !_paused;
                UpdateStatus();
                GetViewport().SetInputAsHandled();
                break;
            case Key.R:
                GetTree().ReloadCurrentScene();
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    // ----- setup -----

    private void SpawnUnits()
    {
        _unitsRoot = new Node2D { Name = "Units" };
        AddChild(_unitsRoot);

        foreach (Marine marine in _scenario.Marines)
        {
            AddUnitView(marine.Id, UnitKind.Marine, marine.MaxHp, marine.Hp, marine.Position);
        }

        foreach (Hostile hostile in _scenario.Hostiles)
        {
            UnitKind kind = hostile.Enemy.Id == "cyst_spitter" ? UnitKind.Spitter : UnitKind.Swarmer;
            AddUnitView(hostile.Id, kind, hostile.MaxHp, hostile.Hp, hostile.Position);
        }
    }

    private void AddUnitView(string id, UnitKind kind, int maxHp, int hp, HexCoord position)
    {
        var view = new UnitView { Position = _boardView.CenterOf(position) };
        _unitsRoot.AddChild(view);
        view.Configure(id, kind, maxHp, hp);
        _units[id] = view;
    }

    // ----- playback -----

    private async Task RunAsync()
    {
        await Wait(0.6);
        while (IsInstanceValid(this) && _scenario.Status == ScenarioStatus.InProgress)
        {
            await PauseGate();
            IReadOnlyList<GameEvent> events = _scenario.PlayRound(_controller);

            foreach (GameEvent gameEvent in events)
            {
                if (!IsInstanceValid(this))
                {
                    return;
                }

                await PauseGate();
                await Animate(gameEvent);
            }

            await Wait(RoundGap);
        }

        UpdateStatus();
    }

    private async Task Animate(GameEvent gameEvent)
    {
        switch (gameEvent)
        {
            case RoundStarted e:
                AddLog($"── Round {e.Round} ──");
                break;

            case EnemyCardDrawn e:
                AddLog($"{e.EnemyTypeId} draws {e.AiCardId} (init {e.Initiative})");
                break;

            case UnitMoved e when _units.TryGetValue(e.UnitId, out UnitView? view):
            {
                Tween tween = CreateTween();
                tween.TweenProperty(view, "position", _boardView.CenterOf(e.To), MoveDuration);
                await ToSignal(tween, Tween.SignalName.Finished);
                return;
            }

            case AttackResolved e:
                await AnimateAttack(e);
                return;

            case ConditionDamage e when _units.TryGetValue(e.UnitId, out UnitView? view):
                view.SetHp(e.RemainingHp);
                AddLog($"{Short(e.UnitId)} takes {e.Amount} (wound)");
                await view.Flash(HitFlash);
                if (e.Killed)
                {
                    await Remove(e.UnitId);
                }

                return;

            case HealApplied e when _units.TryGetValue(e.UnitId, out UnitView? view):
                view.SetHp(e.RemainingHp);
                AddLog($"{Short(e.UnitId)} heals {e.Amount}");
                await view.Flash(HealFlash);
                return;

            case ConditionApplied e when _units.TryGetValue(e.UnitId, out UnitView? view):
                view.AddCondition(e.Condition);
                AddLog($"{Short(e.UnitId)} → {e.Condition}");
                break;

            case ConditionExpired e when _units.TryGetValue(e.UnitId, out UnitView? view):
                view.RemoveCondition(e.Condition);
                break;

            case TurnSkippedStunned e when _units.TryGetValue(e.UnitId, out UnitView? view):
                view.RemoveCondition(BoardingStrike.Game.Cards.ConditionKind.Stunned);
                AddLog($"{Short(e.UnitId)} is stunned, skips");
                break;

            case DoorChanged e:
                _boardView.Refresh();
                AddLog(e.Open ? "a door opens" : "a door closes");
                break;

            case MarineExhausted e:
                AddLog($"{Short(e.MarineId)} is down");
                await Remove(e.MarineId);
                return;

            case ScenarioEnded e:
                AddLog($"Scenario: {e.Status} (round {e.Rounds})");
                UpdateStatus();
                break;
        }

        await Wait(StepPause);
    }

    private async Task AnimateAttack(AttackResolved e)
    {
        if (_units.TryGetValue(e.AttackerId, out UnitView? attacker))
        {
            _ = attacker.Flash(AttackerFlash);
        }

        AddLog($"{Short(e.AttackerId)} hits {Short(e.TargetId)} for {e.DealtDamage}");

        if (_units.TryGetValue(e.TargetId, out UnitView? target))
        {
            target.SetHp(e.TargetRemainingHp);
            await target.Flash(HitFlash);
            if (e.Killed)
            {
                await Remove(e.TargetId);
            }
        }
        else
        {
            await Wait(StepPause);
        }
    }

    private async Task Remove(string unitId)
    {
        if (_units.Remove(unitId, out UnitView? view))
        {
            await view.FadeOutAndFree();
        }
    }

    // ----- helpers -----

    private async Task PauseGate()
    {
        while (_paused && IsInstanceValid(this))
        {
            await Wait(0.1);
        }
    }

    private async Task Wait(double seconds) =>
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);

    private void UpdateStatus()
    {
        if (_scenario is null)
        {
            return;
        }

        int hostiles = 0;
        foreach (Hostile h in _scenario.Hostiles)
        {
            if (h.IsAlive)
            {
                hostiles++;
            }
        }

        int marines = 0;
        foreach (Marine m in _scenario.Marines)
        {
            if (m.IsActive)
            {
                marines++;
            }
        }

        string state = _scenario.Status switch
        {
            ScenarioStatus.Victory => "  ✦ VICTORY",
            ScenarioStatus.Failure => "  ✦ FAILURE",
            _ => _paused ? "  (paused)" : string.Empty,
        };

        _statusLabel.Text =
            $"{_scenario.MissionName}   ·   round {_scenario.Round}{state}\n"
            + $"{marines} marines · {hostiles} hostiles    (F1 coords · Space pause · R restart)";
    }

    private void AddLog(string line)
    {
        _log.Add(line);
        if (_log.Count > 9)
        {
            _log.RemoveAt(0);
        }

        _logLabel.Text = string.Join("\n", _log);
    }

    private static string Short(string unitId)
    {
        if (unitId.StartsWith("marine_", StringComparison.Ordinal))
        {
            return "M" + unitId["marine_".Length..];
        }

        if (unitId.StartsWith("hostile_", StringComparison.Ordinal))
        {
            return "H" + unitId["hostile_".Length..];
        }

        return unitId;
    }

    private Label CreateLogLabel()
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0.5f),
            ContentMarginLeft = 10,
            ContentMarginTop = 6,
            ContentMarginRight = 10,
            ContentMarginBottom = 6,
        };

        var label = new Label
        {
            OffsetLeft = 10,
            OffsetTop = 80,
            OffsetRight = 360,
            OffsetBottom = 320,
        };
        label.AddThemeStyleboxOverride("normal", style);
        label.AddThemeFontSizeOverride("font_size", 13);
        GetNode<CanvasLayer>("Hud").AddChild(label);
        return label;
    }

    private void FrameCamera()
    {
        bool any = false;
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (HexCoord cell in _scenario.Board.FloorCells)
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
