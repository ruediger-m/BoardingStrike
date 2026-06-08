using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Ai;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Combat;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Events;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Scenario;

/// <summary>
/// Runs one round of the scenario per docs/design/core-loop.md: commit phase
/// (marines commit two cards or refresh; enemy types draw an AI card),
/// initiative reveal with the documented tie-breaks, turn execution in order,
/// then the end-of-round tick and victory/failure check.
/// </summary>
public sealed class RoundResolver
{
    private const int RefreshInitiative = 99;

    private readonly ScenarioState _state;
    private readonly IScenarioView _view;

    public RoundResolver(ScenarioState state, IScenarioView view)
    {
        _state = state;
        _view = view;
    }

    private BoardState Board => _state.Board;

    public IReadOnlyList<GameEvent> PlayRound(IMarineController controller)
    {
        var log = new List<GameEvent>();
        if (_state.Status != ScenarioStatus.InProgress)
        {
            return log;
        }

        _state.Round++;
        log.Add(new RoundStarted(_state.Round));

        var resolver = new ActionResolver(Board, _state.Rng, log);
        var ai = new EnemyAi(Board, resolver);

        List<MarineActor> marineActors = CommitMarines(controller, log);
        List<EnemyActor> enemyActors = DrawEnemyCards(log);

        var actors = new List<Actor>();
        actors.AddRange(marineActors);
        actors.AddRange(enemyActors);
        actors.Sort(CompareActors);

        foreach (Actor actor in actors)
        {
            if (actor is MarineActor m)
            {
                ExecuteMarineTurn(m, controller, resolver, log);
            }
            else if (actor is EnemyActor e)
            {
                ExecuteEnemyType(e, ai, resolver, log);
            }

            if (CheckEnd())
            {
                break;
            }
        }

        if (_state.Status == ScenarioStatus.InProgress)
        {
            EndOfRoundTick(log);
            CheckEnd();
        }

        log.Add(new RoundEnded(_state.Round));
        if (_state.Status != ScenarioStatus.InProgress)
        {
            log.Add(new ScenarioEnded(_state.Status.ToString(), _state.Round));
        }

        return log;
    }

    // ----- commit phase -----

    private List<MarineActor> CommitMarines(IMarineController controller, List<GameEvent> log)
    {
        var actors = new List<MarineActor>();
        int order = 0;
        foreach (Marine marine in _state.Marines)
        {
            if (!marine.IsActive)
            {
                continue;
            }

            bool canPlay = marine.Hand.Count >= 2;
            bool canRefresh = marine.Discard.Count > 0;

            MarineCommit commit;
            if (canPlay)
            {
                commit = controller.Commit(marine, _view);
                if (commit is RefreshCommit && !canRefresh)
                {
                    throw new InvalidOperationException($"{marine.Id} cannot refresh with an empty discard.");
                }
            }
            else if (canRefresh)
            {
                commit = new RefreshCommit();
            }
            else
            {
                marine.IsExhausted = true;
                Board.RemoveUnit(marine);
                log.Add(new MarineExhausted(marine.Id));
                continue;
            }

            if (commit is PlayCommit play)
            {
                Card cardA = FindHandCard(marine, play.CardAId);
                Card cardB = FindHandCard(marine, play.CardBId);
                if (ReferenceEquals(cardA, cardB))
                {
                    throw new InvalidOperationException($"{marine.Id} committed the same card twice.");
                }

                marine.Hand.Remove(cardA);
                marine.Hand.Remove(cardB);
                int initiative = Math.Min(cardA.Initiative, cardB.Initiative);
                actors.Add(new MarineActor { Initiative = initiative, Marine = marine, Commit = play, CardA = cardA, CardB = cardB, Order = order++ });
                log.Add(new CardsCommitted(marine.Id, cardA.Id, cardB.Id, initiative));
            }
            else
            {
                actors.Add(new MarineActor { Initiative = RefreshInitiative, Marine = marine, Commit = commit, Order = order++ });
            }
        }

        return actors;
    }

    private List<EnemyActor> DrawEnemyCards(List<GameEvent> log)
    {
        var actors = new List<EnemyActor>();
        foreach (string typeId in _state.LivingEnemyTypeIds)
        {
            AiCard card = _state.AiDecks[typeId].Draw();
            actors.Add(new EnemyActor { Initiative = card.Initiative, TypeId = typeId, Card = card });
            log.Add(new EnemyCardDrawn(typeId, card.Id, card.Initiative));
        }

        return actors;
    }

    // ----- execution -----

    private void ExecuteMarineTurn(MarineActor actor, IMarineController controller, ActionResolver resolver, List<GameEvent> log)
    {
        Marine marine = actor.Marine;
        if (!marine.IsActive)
        {
            return; // killed before its turn
        }

        log.Add(new TurnStarted(marine.Id, actor.Initiative));

        if (TickWoundedAndDie(marine, log))
        {
            return;
        }

        if (marine.Conditions.Has(ConditionKind.Stunned))
        {
            marine.Conditions.Remove(ConditionKind.Stunned);
            log.Add(new TurnSkippedStunned(marine.Id));
            if (actor.Commit is PlayCommit)
            {
                marine.Discard.Add(actor.CardA!);
                marine.Discard.Add(actor.CardB!);
            }

            return;
        }

        if (actor.Commit is RefreshCommit)
        {
            Refresh(marine, log);
            return;
        }

        var play = (PlayCommit)actor.Commit;
        MarinePlan plan = controller.PlanTurn(marine, play, _view);
        ExecutePlay(marine, actor, plan, resolver, log);
    }

    private void ExecutePlay(Marine marine, MarineActor actor, MarinePlan plan, ActionResolver resolver, List<GameEvent> log)
    {
        Card topCard = MatchCommitted(actor, plan.TopCardId);
        Card bottomCard = MatchCommitted(actor, plan.BottomCardId);

        void RunTop() => ExecuteHalf(marine, topCard.Top, plan.TopTargets, resolver);
        void RunBottom() => ExecuteHalf(marine, bottomCard.Bottom, plan.BottomTargets, resolver);

        if (plan.TopFirst)
        {
            RunTop();
            if (marine.IsActive)
            {
                RunBottom();
            }
        }
        else
        {
            RunBottom();
            if (marine.IsActive)
            {
                RunTop();
            }
        }

        RouteCard(marine, topCard, topCard.Top.Burn, log);
        RouteCard(marine, bottomCard, bottomCard.Bottom.Burn, log);
    }

    private void ExecuteHalf(Marine marine, CardHalf half, IReadOnlyList<EffectTarget> targets, ActionResolver resolver)
    {
        for (int i = 0; i < half.Effects.Count; i++)
        {
            if (!marine.IsActive)
            {
                return;
            }

            EffectTarget target = i < targets.Count ? targets[i] : EffectTarget.None;
            ExecuteMarineEffect(marine, half.Effects[i], target, resolver);
        }
    }

    private void ExecuteMarineEffect(Marine marine, ActionEffect effect, EffectTarget target, ActionResolver resolver)
    {
        switch (effect.Kind)
        {
            case ActionKind.Move:
                if (!marine.Conditions.Has(ConditionKind.Immobilized) && target.Destination is HexCoord dest)
                {
                    HexCoord step = Movement.StepToward(Board, marine.Position, dest, effect.Value);
                    resolver.MoveTo(marine, step);
                }

                break;

            case ActionKind.AttackMelee:
            {
                Unit? t = FindUnit(target.TargetUnitId);
                if (t is { IsAlive: true } && marine.Position.DistanceTo(t.Position) == 1 && !Board.BlocksMovement(marine.Position, t.Position))
                {
                    resolver.ResolveAttack(marine, t, effect.Value, effect.Condition);
                }

                break;
            }

            case ActionKind.AttackRanged:
            {
                Unit? t = FindUnit(target.TargetUnitId);
                if (t is { IsAlive: true } && marine.Position.DistanceTo(t.Position) <= effect.Range
                    && Core.Fov.LineOfSight.HasLineOfSight(Board, marine.Position, t.Position))
                {
                    resolver.ResolveAttack(marine, t, effect.Value, effect.Condition);
                }

                break;
            }

            case ActionKind.Heal:
                resolver.Heal(marine, effect.Value);
                break;

            case ActionKind.ApplyCondition:
            {
                Unit? t = FindUnit(target.TargetUnitId);
                if (t is { IsAlive: true } && effect.Condition is ConditionKind c && marine.Position.DistanceTo(t.Position) == 1)
                {
                    resolver.ApplyCondition(t, c);
                }

                break;
            }

            case ActionKind.Door:
                if (target.Door is HexEdge edge && effect.Door is DoorOperation op && BoardHasDoor(edge))
                {
                    switch (op)
                    {
                        case DoorOperation.Open:
                            resolver.SetDoor(edge, true);
                            break;
                        case DoorOperation.Close:
                            resolver.SetDoor(edge, false);
                            break;
                        default:
                            resolver.ToggleDoor(edge);
                            break;
                    }
                }

                break;

            default:
                break;
        }
    }

    private void ExecuteEnemyType(EnemyActor actor, EnemyAi ai, ActionResolver resolver, List<GameEvent> log)
    {
        var members = _state.Hostiles
            .Where(h => h.Enemy.Id == actor.TypeId && h.IsAlive)
            .OrderBy(h => h.SpawnOrder)
            .ToList();

        foreach (Hostile hostile in members)
        {
            if (!hostile.IsAlive)
            {
                continue;
            }

            log.Add(new TurnStarted(hostile.Id, actor.Initiative));

            if (TickWoundedAndDie(hostile, log))
            {
                continue;
            }

            if (hostile.Conditions.Has(ConditionKind.Stunned))
            {
                hostile.Conditions.Remove(ConditionKind.Stunned);
                log.Add(new TurnSkippedStunned(hostile.Id));
                continue;
            }

            ai.Execute(hostile, actor.Card, _state.Marines);
            if (CheckEnd())
            {
                return;
            }
        }
    }

    // ----- helpers -----

    private void Refresh(Marine marine, List<GameEvent> log)
    {
        marine.Hand.AddRange(marine.Discard);
        marine.Discard.Clear();
        if (marine.Hand.Count == 0)
        {
            return;
        }

        int index = _state.Rng.NextInt(marine.Hand.Count);
        Card burned = marine.Hand[index];
        marine.Hand.RemoveAt(index);
        marine.Burned.Add(burned);
        log.Add(new MarineRefreshed(marine.Id, burned.Id));
        log.Add(new CardBurned(marine.Id, burned.Id));
    }

    private static void RouteCard(Marine marine, Card card, bool burned, List<GameEvent> log)
    {
        if (burned)
        {
            marine.Burned.Add(card);
            log.Add(new CardBurned(marine.Id, card.Id));
        }
        else
        {
            marine.Discard.Add(card);
        }
    }

    /// <summary>Applies the start-of-turn wounded tick; returns true if the unit died.</summary>
    private bool TickWoundedAndDie(Unit unit, List<GameEvent> log)
    {
        if (!unit.Conditions.Has(ConditionKind.Wounded))
        {
            return false;
        }

        unit.TakeDamage(1);
        bool killed = !unit.IsAlive;
        log.Add(new ConditionDamage(unit.Id, 1, killed, unit.Hp));
        if (!killed)
        {
            return false;
        }

        Board.RemoveUnit(unit);
        if (unit is Marine marine)
        {
            marine.IsExhausted = true;
            log.Add(new MarineExhausted(marine.Id));
        }

        return true;
    }

    private void EndOfRoundTick(List<GameEvent> log)
    {
        // Immobilized clears at end of the round it was applied (stunned persists
        // until it skips a turn). No damage-over-time conditions in the MVP.
        foreach (Unit unit in AllUnits())
        {
            if (unit.Conditions.Remove(ConditionKind.Immobilized))
            {
                log.Add(new ConditionExpired(unit.Id, ConditionKind.Immobilized));
            }
        }
    }

    private bool CheckEnd()
    {
        if (!_state.AnyHostileAlive)
        {
            _state.Status = ScenarioStatus.Victory;
            return true;
        }

        if (!_state.AnyMarineActive)
        {
            _state.Status = ScenarioStatus.Failure;
            return true;
        }

        return false;
    }

    private IEnumerable<Unit> AllUnits()
    {
        foreach (Marine m in _state.Marines)
        {
            if (m.IsActive)
            {
                yield return m;
            }
        }

        foreach (Hostile h in _state.Hostiles)
        {
            if (h.IsAlive)
            {
                yield return h;
            }
        }
    }

    private Unit? FindUnit(string? id)
    {
        if (id is null)
        {
            return null;
        }

        return (Unit?)_state.Hostiles.FirstOrDefault(h => h.Id == id) ?? _state.Marines.FirstOrDefault(m => m.Id == id);
    }

    private bool BoardHasDoor(HexEdge edge) => Board.Doors.Contains(edge);

    private static Card FindHandCard(Marine marine, string cardId)
    {
        Card? card = marine.Hand.FirstOrDefault(c => c.Id == cardId);
        return card ?? throw new InvalidOperationException($"{marine.Id} committed card '{cardId}' not in hand.");
    }

    private static Card MatchCommitted(MarineActor actor, string cardId)
    {
        if (actor.CardA!.Id == cardId)
        {
            return actor.CardA;
        }

        if (actor.CardB!.Id == cardId)
        {
            return actor.CardB;
        }

        throw new InvalidOperationException($"Plan references card '{cardId}' that was not committed.");
    }

    private static int CompareActors(Actor x, Actor y)
    {
        int c = x.Initiative.CompareTo(y.Initiative);
        if (c != 0)
        {
            return c;
        }

        // Marines act before enemies on an initiative tie.
        int fx = x is MarineActor ? 0 : 1;
        int fy = y is MarineActor ? 0 : 1;
        c = fx.CompareTo(fy);
        if (c != 0)
        {
            return c;
        }

        if (x is MarineActor mx && y is MarineActor my)
        {
            return mx.Order.CompareTo(my.Order);
        }

        if (x is EnemyActor ex && y is EnemyActor ey)
        {
            return string.CompareOrdinal(ex.TypeId, ey.TypeId);
        }

        return 0;
    }

    private abstract class Actor
    {
        public int Initiative { get; init; }
    }

    private sealed class MarineActor : Actor
    {
        public required Marine Marine { get; init; }

        public required MarineCommit Commit { get; init; }

        public Card? CardA { get; init; }

        public Card? CardB { get; init; }

        public int Order { get; init; }
    }

    private sealed class EnemyActor : Actor
    {
        public required string TypeId { get; init; }

        public required AiCard Card { get; init; }
    }
}
