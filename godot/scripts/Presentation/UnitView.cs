using System.Threading.Tasks;
using Godot;
using BoardingStrike.Game.Cards;

namespace BoardingStrike.Presentation;

public enum UnitKind
{
    Marine,
    Swarmer,
    Spitter,
}

/// <summary>
/// A single unit's visual: a placeholder marker (shape/colour by kind), an HP
/// bar, and small condition dots. Its <see cref="Node2D.Position"/> is the hex
/// pixel center, so movement is a position tween. Driven entirely by the
/// scenario event stream — it holds no rules state of its own.
/// </summary>
public partial class UnitView : Node2D
{
    private static readonly Color MarineColor = new("ff7a2f");
    private static readonly Color SwarmerColor = new("7fd44a");
    private static readonly Color SpitterColor = new("a96bd6");
    private static readonly Color HpBack = new("000000", 0.6f);
    private static readonly Color HpFull = new("7fd44a");
    private static readonly Color HpLow = new("e5484d");

    private UnitKind _kind;
    private int _maxHp = 1;
    private int _hp = 1;
    private readonly System.Collections.Generic.HashSet<ConditionKind> _conditions = [];

    public string UnitId { get; private set; } = string.Empty;

    public void Configure(string id, UnitKind kind, int maxHp, int hp)
    {
        UnitId = id;
        _kind = kind;
        _maxHp = System.Math.Max(1, maxHp);
        _hp = hp;
        QueueRedraw();
    }

    public void SetHp(int hp)
    {
        _hp = hp;
        QueueRedraw();
    }

    public void AddCondition(ConditionKind condition)
    {
        if (_conditions.Add(condition))
        {
            QueueRedraw();
        }
    }

    public void RemoveCondition(ConditionKind condition)
    {
        if (_conditions.Remove(condition))
        {
            QueueRedraw();
        }
    }

    /// <summary>Briefly tints the marker, then returns to normal.</summary>
    public async Task Flash(Color color)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate", color, 0.07);
        tween.TweenProperty(this, "modulate", Colors.White, 0.10);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    /// <summary>Fades the marker out and frees the node.</summary>
    public async Task FadeOutAndFree()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, 0.18);
        await ToSignal(tween, Tween.SignalName.Finished);
        QueueFree();
    }

    public override void _Draw()
    {
        const float size = BoardView.HexSize;
        DrawMarker(size);
        DrawHpBar(size);
        DrawConditions(size);
    }

    private void DrawMarker(float size)
    {
        switch (_kind)
        {
            case UnitKind.Marine:
            {
                float h = size * 0.5f;
                DrawRect(new Rect2(new Vector2(-h, -h), new Vector2(h * 2, h * 2)), MarineColor);
                break;
            }

            case UnitKind.Spitter:
            {
                float r = size * 0.6f;
                DrawColoredPolygon(
                    [new Vector2(0, -r), new Vector2(r, 0), new Vector2(0, r), new Vector2(-r, 0)],
                    SpitterColor);
                break;
            }

            default: // Swarmer
            {
                float r = size * 0.6f;
                DrawColoredPolygon([new Vector2(0, -r), new Vector2(r, r), new Vector2(-r, r)], SwarmerColor);
                break;
            }
        }
    }

    private void DrawHpBar(float size)
    {
        float ratio = Mathf.Clamp((float)_hp / _maxHp, 0f, 1f);
        float width = size * 1.3f;
        float height = 4f;
        var origin = new Vector2(-width * 0.5f, -size - 8f);
        DrawRect(new Rect2(origin, new Vector2(width, height)), HpBack);
        DrawRect(new Rect2(origin, new Vector2(width * ratio, height)), HpLow.Lerp(HpFull, ratio));
    }

    private void DrawConditions(float size)
    {
        float x = -size * 0.5f;
        float y = size + 4f;
        foreach (ConditionKind condition in _conditions)
        {
            DrawCircle(new Vector2(x, y), 3f, ConditionColor(condition));
            x += 9f;
        }
    }

    private static Color ConditionColor(ConditionKind condition) => condition switch
    {
        ConditionKind.Wounded => new Color("e5484d"),
        ConditionKind.Immobilized => new Color("4a90e2"),
        ConditionKind.Stunned => new Color("f5d800"),
        _ => new Color("cccccc"),
    };
}
