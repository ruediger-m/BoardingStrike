using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Cards;

namespace BoardingStrike.Game.Events;

/// <summary>
/// A read-only record of something that happened in the rules engine. The
/// engine moves state immediately and appends events describing it; the
/// presentation layer (later) replays the event stream to animate. Tests assert
/// against it.
/// </summary>
public abstract record GameEvent;

public sealed record RoundStarted(int Round) : GameEvent;

public sealed record RoundEnded(int Round) : GameEvent;

public sealed record EnemyCardDrawn(string EnemyTypeId, string AiCardId, int Initiative) : GameEvent;

public sealed record CardsCommitted(string MarineId, string CardAId, string CardBId, int Initiative) : GameEvent;

public sealed record MarineRefreshed(string MarineId, string BurnedCardId) : GameEvent;

public sealed record MarineExhausted(string MarineId) : GameEvent;

public sealed record TurnStarted(string UnitId, int Initiative) : GameEvent;

public sealed record TurnSkippedStunned(string UnitId) : GameEvent;

public sealed record UnitMoved(string UnitId, HexCoord From, HexCoord To) : GameEvent;

public sealed record AttackResolved(
    string AttackerId,
    string TargetId,
    int BaseDamage,
    int ModifiedDamage,
    int DealtDamage,
    bool Killed) : GameEvent;

public sealed record HealApplied(string UnitId, int Amount) : GameEvent;

public sealed record ConditionApplied(string UnitId, ConditionKind Condition) : GameEvent;

public sealed record ConditionExpired(string UnitId, ConditionKind Condition) : GameEvent;

public sealed record ConditionDamage(string UnitId, int Amount, bool Killed) : GameEvent;

public sealed record DoorChanged(HexEdge Edge, bool Open) : GameEvent;

public sealed record CardBurned(string MarineId, string CardId) : GameEvent;

public sealed record ScenarioEnded(string Status, int Rounds) : GameEvent;
