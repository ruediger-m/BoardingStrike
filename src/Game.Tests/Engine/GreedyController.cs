using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Scenario;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Tests.Engine;

/// <summary>
/// A deterministic, door-aware greedy marine controller for the integration
/// smoke test. Each turn it heads for the nearest engageable hostile; when no
/// hostile is reachable (a closed door is in the way) it walks to the nearest
/// reachable door, and once standing on the door it commits the breach card to
/// open it. Not "smart" — just enough to exercise the whole engine against the
/// real Hangar Sweep content and drive it to a terminal state.
/// </summary>
internal sealed class GreedyController : IMarineController
{
    private const string BreachCardId = "marine_breach_and_clear";

    public MarineCommit Commit(Marine marine, IScenarioView view)
    {
        List<Card> hand = marine.Hand;
        bool onClosedDoor = AdjacentClosedDoor(marine, view) is not null;

        Card a, b;
        if (onClosedDoor && hand.FirstOrDefault(c => c.Id == BreachCardId) is Card breach)
        {
            // Standing on a door: use the breach card (its top carries the door op) to open it.
            a = breach;
            b = hand.First(c => !ReferenceEquals(c, a));
        }
        else
        {
            // Otherwise approach: an attack card up top, a move-capable card on the bottom.
            a = hand.FirstOrDefault(c => c.Id != BreachCardId && IsAttack(c.Top)) ?? hand.First(c => c.Id != BreachCardId);
            b = hand.FirstOrDefault(c => !ReferenceEquals(c, a) && IsMove(c.Bottom))
                ?? hand.First(c => !ReferenceEquals(c, a));
        }

        return new PlayCommit(a.Id, b.Id);
    }

    public MarinePlan PlanTurn(Marine marine, PlayCommit committed, IScenarioView view)
    {
        Card top = marine.Class.StartingCards.First(c => c.Id == committed.CardAId);
        Card bottom = marine.Class.StartingCards.First(c => c.Id == committed.CardBId);

        HexCoord goal = ChooseGoal(marine, view);
        Hostile? attackTarget = view.Hostiles
            .Where(h => h.IsAlive)
            .OrderBy(h => marine.Position.DistanceTo(h.Position))
            .ThenBy(h => h.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        return new MarinePlan(
            committed.CardAId,
            committed.CardBId,
            TopFirst: true,
            BuildTargets(marine, view, top.Top, goal, attackTarget),
            BuildTargets(marine, view, bottom.Bottom, goal, attackTarget));
    }

    private static IReadOnlyList<EffectTarget> BuildTargets(
        Marine marine, IScenarioView view, CardHalf half, HexCoord goal, Hostile? attackTarget)
    {
        var targets = new List<EffectTarget>(half.Effects.Count);
        foreach (ActionEffect effect in half.Effects)
        {
            targets.Add(effect.Kind switch
            {
                ActionKind.Move => EffectTarget.ToHex(goal),
                ActionKind.AttackMelee or ActionKind.AttackRanged or ActionKind.ApplyCondition =>
                    attackTarget is null ? EffectTarget.None : EffectTarget.Unit(attackTarget.Id),
                ActionKind.Door => AdjacentClosedDoor(marine, view) is HexEdge edge
                    ? EffectTarget.DoorEdge(edge)
                    : EffectTarget.None,
                _ => EffectTarget.None,
            });
        }

        return targets;
    }

    /// <summary>
    /// Where the marine should move: toward the nearest hostile it can actually
    /// reach, otherwise toward the nearest reachable closed door to breach it.
    /// </summary>
    private static HexCoord ChooseGoal(Marine marine, IScenarioView view)
    {
        BoardState board = view.Board;
        Dictionary<HexCoord, int> reachable = Movement.ReachableWithin(board, marine.Position, 99);

        Hostile? engageable = view.Hostiles
            .Where(h => h.IsAlive && h.Position.Neighbors().Any(reachable.ContainsKey))
            .OrderBy(h => reachable.GetValueOrDefault(NearestReachableNeighbor(h, reachable), int.MaxValue))
            .ThenBy(h => h.Id, StringComparer.Ordinal)
            .FirstOrDefault();
        if (engageable is not null)
        {
            return engageable.Position;
        }

        // No hostile reachable — walk to the nearest reachable closed-door endpoint.
        HexCoord? doorGoal = board.Doors
            .Where(e => !board.IsDoorOpen(e.A, e.B))
            .SelectMany(e => new[] { e.A, e.B })
            .Where(reachable.ContainsKey)
            .OrderBy(c => reachable[c]).ThenBy(c => c.Q).ThenBy(c => c.R)
            .Cast<HexCoord?>()
            .FirstOrDefault();

        return doorGoal ?? marine.Position;
    }

    private static HexCoord NearestReachableNeighbor(Hostile hostile, IReadOnlyDictionary<HexCoord, int> reachable) =>
        hostile.Position.Neighbors()
            .Where(reachable.ContainsKey)
            .OrderBy(n => reachable[n])
            .First();

    private static HexEdge? AdjacentClosedDoor(Marine marine, IScenarioView view) =>
        view.Board.Doors
            .Where(e => (e.A == marine.Position || e.B == marine.Position) && !view.Board.IsDoorOpen(e.A, e.B))
            .OrderBy(e => e.A.Q).ThenBy(e => e.A.R).ThenBy(e => e.B.Q).ThenBy(e => e.B.R)
            .Cast<HexEdge?>()
            .FirstOrDefault();

    private static bool IsAttack(CardHalf half) =>
        half.Effects.Any(e => e.Kind is ActionKind.AttackMelee or ActionKind.AttackRanged or ActionKind.AttackArea);

    private static bool IsMove(CardHalf half) => half.Effects.Any(e => e.Kind == ActionKind.Move);
}
