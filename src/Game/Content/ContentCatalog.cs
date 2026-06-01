using BoardingStrike.Core.Data;
using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Combat;

namespace BoardingStrike.Game.Content;

/// <summary>
/// The domain-side catalog of content, built from a validated
/// <see cref="ContentDatabase"/> of DTOs. This is the Step-5 mapping that the
/// loader deliberately deferred (so Core never had to reference Game). Once
/// built, the rest of the engine works with rich domain entities, not DTOs.
/// </summary>
public sealed class ContentCatalog
{
    private ContentCatalog(
        IReadOnlyDictionary<string, Card> cards,
        IReadOnlyDictionary<string, ClassDef> classes,
        IReadOnlyDictionary<string, EnemyDef> enemies,
        IReadOnlyDictionary<string, MissionDef> missions,
        IReadOnlyDictionary<string, MapData> maps)
    {
        Cards = cards;
        Classes = classes;
        Enemies = enemies;
        Missions = missions;
        Maps = maps;
    }

    public IReadOnlyDictionary<string, Card> Cards { get; }

    public IReadOnlyDictionary<string, ClassDef> Classes { get; }

    public IReadOnlyDictionary<string, EnemyDef> Enemies { get; }

    public IReadOnlyDictionary<string, MissionDef> Missions { get; }

    public IReadOnlyDictionary<string, MapData> Maps { get; }

    /// <summary>Builds a catalog from validated content. Loads + validates from disk.</summary>
    public static ContentCatalog Load(string contentRoot) => Build(JsonContentLoader.Load(contentRoot));

    public static ContentCatalog Build(ContentDatabase db)
    {
        ArgumentNullException.ThrowIfNull(db);

        var modifierTemplates = db.ModifierDecks.ToDictionary(
            kv => kv.Key, kv => BuildModifierTemplate(kv.Value), StringComparer.Ordinal);

        var cards = db.Cards.ToDictionary(kv => kv.Key, kv => BuildCard(kv.Value), StringComparer.Ordinal);
        var aiCards = db.AiCards.ToDictionary(kv => kv.Key, kv => BuildAiCard(kv.Value), StringComparer.Ordinal);

        var classes = db.Classes.ToDictionary(
            kv => kv.Key, kv => BuildClass(kv.Value, cards, modifierTemplates), StringComparer.Ordinal);

        var enemies = db.Enemies.ToDictionary(
            kv => kv.Key, kv => BuildEnemy(kv.Value, aiCards, modifierTemplates), StringComparer.Ordinal);

        var missions = db.Missions.ToDictionary(kv => kv.Key, kv => BuildMission(kv.Value), StringComparer.Ordinal);

        return new ContentCatalog(cards, classes, enemies, missions, db.Maps);
    }

    private static ModifierDeckTemplate BuildModifierTemplate(ModifierDeckData data)
    {
        var cards = new List<ModifierCard>();
        foreach (ModifierEntryData entry in data.Cards!)
        {
            var card = new ModifierCard(
                ParseModifierEffect(entry.Effect!),
                entry.Value!.Value,
                entry.Reshuffle ?? false);
            for (int i = 0; i < entry.Count!.Value; i++)
            {
                cards.Add(card);
            }
        }

        return new ModifierDeckTemplate(cards);
    }

    private static Card BuildCard(CardData data) =>
        new(data.Id!, data.Name ?? data.Id!, data.Initiative!.Value, BuildHalf(data.Top!), BuildHalf(data.Bottom!));

    private static CardHalf BuildHalf(CardHalfData half) =>
        new([.. half.Actions!.Select(BuildEffect)], half.Burn ?? false);

    private static AiCard BuildAiCard(AiCardData data) =>
        new(
            data.Id!,
            data.Name ?? data.Id!,
            data.Initiative!.Value,
            data.Movement!.Value,
            [.. (data.Actions ?? []).Select(BuildEffect)],
            ParseTargetPriority(data.TargetPriority!),
            ParseMovementMode(data.MovementMode!));

    private static ActionEffect BuildEffect(ActionData a)
    {
        ActionKind kind = ParseActionKind(a.Kind!);
        return kind switch
        {
            ActionKind.Move => new ActionEffect { Kind = kind, Value = a.Distance ?? 0 },
            ActionKind.AttackMelee => new ActionEffect { Kind = kind, Value = a.Damage ?? 0, Condition = ParseConditionOrNull(a.OnHitCondition) },
            ActionKind.AttackRanged => new ActionEffect { Kind = kind, Value = a.Damage ?? 0, Range = a.Range ?? 0, Condition = ParseConditionOrNull(a.OnHitCondition) },
            ActionKind.AttackArea => new ActionEffect { Kind = kind, Value = a.Damage ?? 0, Range = a.Range ?? 0, Condition = ParseConditionOrNull(a.OnHitCondition) },
            ActionKind.Heal => new ActionEffect { Kind = kind, Value = a.Amount ?? 0 },
            ActionKind.ApplyCondition => new ActionEffect { Kind = kind, Condition = ParseConditionOrNull(a.Condition) },
            ActionKind.Door => new ActionEffect { Kind = kind, Door = ParseDoorOperation(a.Operation!) },
            _ => new ActionEffect { Kind = kind },
        };
    }

    private static ClassDef BuildClass(
        ClassData data,
        IReadOnlyDictionary<string, Card> cards,
        IReadOnlyDictionary<string, ModifierDeckTemplate> modifiers)
    {
        var starting = data.StartingCardIds!.Select(id => cards[id]).ToList();
        return new ClassDef(data.Id!, data.Name ?? data.Id!, data.Hp!.Value, data.Sprite ?? string.Empty, starting, modifiers[data.ModifierDeckId!]);
    }

    private static EnemyDef BuildEnemy(
        EnemyData data,
        IReadOnlyDictionary<string, AiCard> aiCards,
        IReadOnlyDictionary<string, ModifierDeckTemplate> modifiers)
    {
        var deck = data.AiCardIds!.Select(id => aiCards[id]).ToList();
        return new EnemyDef(data.Id!, data.Name ?? data.Id!, data.Hp!.Value, data.Sprite ?? string.Empty, deck, modifiers[data.ModifierDeckId!]);
    }

    private static MissionDef BuildMission(MissionData data) =>
        new(
            data.Id!,
            data.Name ?? data.Id!,
            data.MapId!,
            ParseVictory(data.Victory!.Kind!),
            ParseFailure(data.Failure!.Kind!),
            data.RngSeedSource);

    // ----- enum parsing (content is pre-validated, so unknowns are a programming error) -----

    private static ModifierEffectKind ParseModifierEffect(string s) => s switch
    {
        "multiply" => ModifierEffectKind.Multiply,
        "add" => ModifierEffectKind.Add,
        _ => throw new ArgumentException($"Unknown modifier effect '{s}'."),
    };

    private static ActionKind ParseActionKind(string s) => s switch
    {
        "move" => ActionKind.Move,
        "attack_melee" => ActionKind.AttackMelee,
        "attack_ranged" => ActionKind.AttackRanged,
        "attack_area" => ActionKind.AttackArea,
        "heal" => ActionKind.Heal,
        "apply_condition" => ActionKind.ApplyCondition,
        "door" => ActionKind.Door,
        "loot" => ActionKind.Loot,
        _ => throw new ArgumentException($"Unknown action kind '{s}'."),
    };

    private static ConditionKind? ParseConditionOrNull(string? s) => s switch
    {
        null => null,
        "stunned" => ConditionKind.Stunned,
        "wounded" => ConditionKind.Wounded,
        "immobilized" => ConditionKind.Immobilized,
        "poisoned" => ConditionKind.Poisoned,
        "muddled" => ConditionKind.Muddled,
        "invisible" => ConditionKind.Invisible,
        "strengthen" => ConditionKind.Strengthen,
        _ => throw new ArgumentException($"Unknown condition '{s}'."),
    };

    private static DoorOperation ParseDoorOperation(string s) => s switch
    {
        "open" => DoorOperation.Open,
        "close" => DoorOperation.Close,
        "toggle" => DoorOperation.Toggle,
        _ => throw new ArgumentException($"Unknown door operation '{s}'."),
    };

    private static TargetPriority ParseTargetPriority(string s) => s switch
    {
        "closest_marine" => TargetPriority.ClosestMarine,
        "closest_marine_los" => TargetPriority.ClosestMarineLos,
        "highest_hp_marine_los" => TargetPriority.HighestHpMarineLos,
        "lowest_hp_marine" => TargetPriority.LowestHpMarine,
        "none" => TargetPriority.None,
        _ => throw new ArgumentException($"Unknown target priority '{s}'."),
    };

    private static MovementMode ParseMovementMode(string s) => s switch
    {
        "approach_target" => MovementMode.ApproachTarget,
        "flee_from_closest_marine" => MovementMode.FleeFromClosestMarine,
        "reposition_for_los" => MovementMode.RepositionForLos,
        "reposition_no_los" => MovementMode.RepositionNoLos,
        "static" => MovementMode.Static,
        _ => throw new ArgumentException($"Unknown movement mode '{s}'."),
    };

    private static VictoryKind ParseVictory(string s) => s switch
    {
        "eliminate_all_hostiles" => VictoryKind.EliminateAllHostiles,
        "reach_hex" => VictoryKind.ReachHex,
        "reach_hex_with_radius_cleared" => VictoryKind.ReachHexWithRadiusCleared,
        "hold_hex_for_n_rounds" => VictoryKind.HoldHexForNRounds,
        "escort_to_hex" => VictoryKind.EscortToHex,
        "survive_n_rounds" => VictoryKind.SurviveNRounds,
        "defend_hex_for_n_rounds" => VictoryKind.DefendHexForNRounds,
        _ => throw new ArgumentException($"Unknown victory kind '{s}'."),
    };

    private static FailureKind ParseFailure(string s) => s switch
    {
        "all_marines_exhausted" => FailureKind.AllMarinesExhausted,
        _ => throw new ArgumentException($"Unknown failure kind '{s}'."),
    };
}
