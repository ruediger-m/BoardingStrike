namespace BoardingStrike.Core.Data;

/// <summary>
/// The closed sets of allowed string values for the content schemas, in one
/// place so the validator and (later) the domain mappers agree. Mirrors the
/// enums called out in docs/technical/data-model.md. Reserved-but-not-yet-used
/// values are included so authored content can reference them without tripping
/// validation.
/// </summary>
public static class ContentVocabulary
{
    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.Ordinal);

    public static IReadOnlySet<string> ActionKinds { get; } =
        Set("move", "attack_melee", "attack_ranged", "attack_area", "heal", "apply_condition", "door", "loot");

    public static IReadOnlySet<string> MoveTypes { get; } = Set("walk", "jump", "fly");

    public static IReadOnlySet<string> Conditions { get; } =
        Set("stunned", "wounded", "immobilized", "poisoned", "muddled", "invisible", "strengthen");

    public static IReadOnlySet<string> HealTargets { get; } = Set("self", "adjacent_ally");

    public static IReadOnlySet<string> DoorOperations { get; } = Set("open", "close", "toggle");

    public static IReadOnlySet<string> ModifierEffects { get; } = Set("multiply", "add");

    public static IReadOnlySet<string> TargetPriorities { get; } =
        Set("closest_marine", "closest_marine_los", "highest_hp_marine_los", "lowest_hp_marine", "none");

    public static IReadOnlySet<string> MovementModes { get; } =
        Set("approach_target", "flee_from_closest_marine", "reposition_for_los", "reposition_no_los", "static");

    public static IReadOnlySet<string> EdgeKinds { get; } = Set("wall", "door", "open");

    public static IReadOnlySet<string> EdgeStates { get; } = Set("open", "closed");

    public static IReadOnlySet<string> HexKinds { get; } = Set("floor", "void");

    public static IReadOnlySet<string> VictoryKinds { get; } =
        Set("eliminate_all_hostiles", "reach_hex", "reach_hex_with_radius_cleared",
            "hold_hex_for_n_rounds", "escort_to_hex", "survive_n_rounds", "defend_hex_for_n_rounds");

    public static IReadOnlySet<string> FailureKinds { get; } = Set("all_marines_exhausted");

    public static IReadOnlySet<string> SpawnKinds { get; } = Set("marine_slot", "enemy");

    public static IReadOnlySet<string> RngSeedSources { get; } = Set("scenario_id", "random");
}
