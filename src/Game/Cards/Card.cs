namespace BoardingStrike.Game.Cards;

public enum ActionKind
{
    Move,
    AttackMelee,
    AttackRanged,
    AttackArea,
    Heal,
    ApplyCondition,
    Door,
    Loot,
}

public enum ConditionKind
{
    Stunned,
    Wounded,
    Immobilized,
    Poisoned,
    Muddled,
    Invisible,
    Strengthen,
}

public enum DoorOperation
{
    Open,
    Close,
    Toggle,
}

/// <summary>
/// One resolved action primitive (the domain form of <c>ActionData</c>). A flat
/// shape covering every kind; only the fields relevant to <see cref="Kind"/> are
/// meaningful.
/// </summary>
public sealed record ActionEffect
{
    public required ActionKind Kind { get; init; }

    /// <summary>Damage (attacks), distance (move), or amount (heal).</summary>
    public int Value { get; init; }

    /// <summary>Range for ranged/area attacks.</summary>
    public int Range { get; init; }

    /// <summary>On-hit condition for attacks, or the condition for apply_condition.</summary>
    public ConditionKind? Condition { get; init; }

    /// <summary>Operation for door effects.</summary>
    public DoorOperation? Door { get; init; }
}

/// <summary>One half (top or bottom) of an action card.</summary>
public sealed record CardHalf(IReadOnlyList<ActionEffect> Effects, bool Burn);

/// <summary>A player action card (docs/design/action-system.md).</summary>
public sealed class Card
{
    public Card(string id, string name, int initiative, CardHalf top, CardHalf bottom)
    {
        Id = id;
        Name = name;
        Initiative = initiative;
        Top = top;
        Bottom = bottom;
    }

    public string Id { get; }

    public string Name { get; }

    /// <summary>01–99; lower acts earlier in the round.</summary>
    public int Initiative { get; }

    public CardHalf Top { get; }

    public CardHalf Bottom { get; }
}
