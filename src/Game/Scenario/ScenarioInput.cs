using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Scenario;

public enum ScenarioStatus
{
    InProgress,
    Victory,
    Failure,
}

/// <summary>What a marine commits at the start of a round: two cards, or a refresh.</summary>
public abstract record MarineCommit;

/// <summary>Recover the discard pile (burning one card at random) as the whole turn.</summary>
public sealed record RefreshCommit : MarineCommit;

/// <summary>Commit two cards from hand; initiative is the lower of the two.</summary>
public sealed record PlayCommit(string CardAId, string CardBId) : MarineCommit;

/// <summary>
/// The target of a single action primitive, supplied at turn execution. Only the
/// field relevant to the primitive is read (destination for move, unit for
/// attack/condition, door edge for door).
/// </summary>
public sealed record EffectTarget
{
    public HexCoord? Destination { get; init; }

    public string? TargetUnitId { get; init; }

    public HexEdge? Door { get; init; }

    public static EffectTarget None { get; } = new();

    public static EffectTarget ToHex(HexCoord hex) => new() { Destination = hex };

    public static EffectTarget Unit(string unitId) => new() { TargetUnitId = unitId };

    public static EffectTarget DoorEdge(HexEdge edge) => new() { Door = edge };
}

/// <summary>
/// A marine's decision at its turn: which committed card supplies the top action
/// and which supplies the bottom, the execution order, and the per-primitive
/// targets for each half.
/// </summary>
public sealed record MarinePlan(
    string TopCardId,
    string BottomCardId,
    bool TopFirst,
    IReadOnlyList<EffectTarget> TopTargets,
    IReadOnlyList<EffectTarget> BottomTargets);

/// <summary>Read-only view of the scenario passed to controllers for decisions.</summary>
public interface IScenarioView
{
    int Round { get; }

    ScenarioStatus Status { get; }

    BoardState Board { get; }

    IReadOnlyList<Marine> Marines { get; }

    IReadOnlyList<Hostile> Hostiles { get; }
}

/// <summary>
/// Supplies marine decisions to the engine. A UI implements this from player
/// input; tests implement it as a policy or a fixed script.
/// </summary>
public interface IMarineController
{
    MarineCommit Commit(Marine marine, IScenarioView view);

    MarinePlan PlanTurn(Marine marine, PlayCommit committed, IScenarioView view);
}
