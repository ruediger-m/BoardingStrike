using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Combat;

namespace BoardingStrike.Game.Content;

public enum TargetPriority
{
    ClosestMarine,
    ClosestMarineLos,
    HighestHpMarineLos,
    LowestHpMarine,
    None,
}

public enum MovementMode
{
    ApproachTarget,
    FleeFromClosestMarine,
    RepositionForLos,
    RepositionNoLos,
    Static,
}

public enum VictoryKind
{
    EliminateAllHostiles,
    ReachHex,
    ReachHexWithRadiusCleared,
    HoldHexForNRounds,
    EscortToHex,
    SurviveNRounds,
    DefendHexForNRounds,
}

public enum FailureKind
{
    AllMarinesExhausted,
}

/// <summary>A class template: stats, starting deck, and modifier deck.</summary>
public sealed class ClassDef
{
    public ClassDef(string id, string name, int maxHp, string sprite, IReadOnlyList<Card> startingCards, ModifierDeckTemplate modifiers)
    {
        Id = id;
        Name = name;
        MaxHp = maxHp;
        Sprite = sprite;
        StartingCards = startingCards;
        Modifiers = modifiers;
    }

    public string Id { get; }

    public string Name { get; }

    public int MaxHp { get; }

    public string Sprite { get; }

    public IReadOnlyList<Card> StartingCards { get; }

    public ModifierDeckTemplate Modifiers { get; }
}

/// <summary>One enemy AI behavior card (the domain form of <c>AiCardData</c>).</summary>
public sealed class AiCard
{
    public AiCard(string id, string name, int initiative, int movement, IReadOnlyList<ActionEffect> actions, TargetPriority target, MovementMode mode)
    {
        Id = id;
        Name = name;
        Initiative = initiative;
        Movement = movement;
        Actions = actions;
        Target = target;
        Mode = mode;
    }

    public string Id { get; }

    public string Name { get; }

    public int Initiative { get; }

    public int Movement { get; }

    public IReadOnlyList<ActionEffect> Actions { get; }

    public TargetPriority Target { get; }

    public MovementMode Mode { get; }
}

/// <summary>An enemy type: stats, AI deck, modifier deck.</summary>
public sealed class EnemyDef
{
    public EnemyDef(string id, string name, int maxHp, string sprite, IReadOnlyList<AiCard> aiDeck, ModifierDeckTemplate modifiers)
    {
        Id = id;
        Name = name;
        MaxHp = maxHp;
        Sprite = sprite;
        AiDeck = aiDeck;
        Modifiers = modifiers;
    }

    public string Id { get; }

    public string Name { get; }

    public int MaxHp { get; }

    public string Sprite { get; }

    public IReadOnlyList<AiCard> AiDeck { get; }

    public ModifierDeckTemplate Modifiers { get; }
}

/// <summary>A scenario definition.</summary>
public sealed class MissionDef
{
    public MissionDef(string id, string name, string mapId, VictoryKind victory, FailureKind failure, string? rngSeedSource)
    {
        Id = id;
        Name = name;
        MapId = mapId;
        Victory = victory;
        Failure = failure;
        RngSeedSource = rngSeedSource;
    }

    public string Id { get; }

    public string Name { get; }

    public string MapId { get; }

    public VictoryKind Victory { get; }

    public FailureKind Failure { get; }

    public string? RngSeedSource { get; }
}
