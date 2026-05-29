namespace BoardingStrike.Core.Data;

// Data-transfer objects mirroring the JSON content schemas in
// docs/technical/data-model.md. These are *raw* deserialization targets: every
// field is nullable so the loader can distinguish "absent" from "default" and
// emit precise validation errors. They are intentionally dumb — turning them
// into rich domain entities (Card, ClassDef, ...) happens in the Game layer in
// Step 5, which keeps the dependency direction Game -> Core intact.

/// <summary>
/// A single action primitive. Modeled as a flat union keyed by <see cref="Kind"/>
/// (rather than polymorphic types) because hand-authored content is far more
/// forgiving to validate this way, and System.Text.Json polymorphism is
/// sensitive to property ordering. Validation checks that the fields required
/// by a given kind are present.
/// </summary>
public sealed record ActionData
{
    public string? Kind { get; init; }

    // move
    public int? Distance { get; init; }
    public string? Type { get; init; }

    // attack_melee / attack_ranged / attack_area
    public int? Damage { get; init; }
    public int? Range { get; init; }
    public string? OnHitCondition { get; init; }
    public string? Shape { get; init; }

    // heal
    public int? Amount { get; init; }
    public string? Target { get; init; }

    // apply_condition
    public string? Condition { get; init; }

    // door
    public string? Operation { get; init; }
}

/// <summary>One half (top or bottom) of an action card.</summary>
public sealed record CardHalfData
{
    public bool? Burn { get; init; }
    public IReadOnlyList<ActionData>? Actions { get; init; }
}

/// <summary>A player action card (docs/technical/data-model.md "Cards").</summary>
public sealed record CardData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? ClassId { get; init; }
    public int? Level { get; init; }
    public int? Initiative { get; init; }
    public CardHalfData? Top { get; init; }
    public CardHalfData? Bottom { get; init; }
    public string? Flavor { get; init; }
}

/// <summary>A playable class template (docs/technical/data-model.md "Classes").</summary>
public sealed record ClassData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public int? Hp { get; init; }
    public string? Sprite { get; init; }
    public IReadOnlyList<string>? StartingCardIds { get; init; }
    public string? ModifierDeckId { get; init; }
}

/// <summary>One stack of identical modifier cards within a deck.</summary>
public sealed record ModifierEntryData
{
    public string? Effect { get; init; }
    public int? Value { get; init; }
    public bool? Reshuffle { get; init; }
    public int? Count { get; init; }
}

/// <summary>An attack-modifier deck (docs/technical/data-model.md "Modifier decks").</summary>
public sealed record ModifierDeckData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public IReadOnlyList<ModifierEntryData>? Cards { get; init; }
}
