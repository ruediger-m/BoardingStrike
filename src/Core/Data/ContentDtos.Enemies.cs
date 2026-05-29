namespace BoardingStrike.Core.Data;

/// <summary>An enemy type (docs/technical/data-model.md "Enemies").</summary>
public sealed record EnemyData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Sprite { get; init; }
    public int? Hp { get; init; }
    public string? ModifierDeckId { get; init; }
    public IReadOnlyList<string>? AiCardIds { get; init; }
}

/// <summary>
/// A single enemy AI behavior card (docs/technical/data-model.md "AI cards").
/// <see cref="Actions"/> may be empty — some cards (e.g. a pure reposition) only
/// move.
/// </summary>
public sealed record AiCardData
{
    public int? Version { get; init; }
    public string? Id { get; init; }
    public string? Name { get; init; }
    public int? Initiative { get; init; }
    public int? Movement { get; init; }
    public IReadOnlyList<ActionData>? Actions { get; init; }
    public string? TargetPriority { get; init; }
    public string? MovementMode { get; init; }
}
