using BoardingStrike.Core.Fov;
using BoardingStrike.Core.Hex;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Combat;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Ai;

/// <summary>
/// Executes one hostile's turn from its drawn AI card (docs/design/enemies.md):
/// pick a target by priority, move per the card's movement mode, then resolve
/// the card's actions. All choices are deterministic given the board state.
///
/// <para>MVP approximations (flagged for later tuning): target/movement scoring
/// uses hex distance rather than full path distance, and the Spitter's "Inhale"
/// (+2 to the next attack) and "Acid Volley"'s distinct second target are not
/// yet modeled — Inhale simply costs the turn, which preserves the telegraph.</para>
/// </summary>
public sealed class EnemyAi
{
    private readonly BoardState _board;
    private readonly ActionResolver _resolver;

    public EnemyAi(BoardState board, ActionResolver resolver)
    {
        _board = board;
        _resolver = resolver;
    }

    public void Execute(Hostile hostile, AiCard card, IReadOnlyList<Marine> marines)
    {
        var active = marines.Where(m => m.IsActive).ToList();
        if (active.Count == 0)
        {
            return;
        }

        Marine? primary = SelectTarget(card.Target, hostile, active);

        // Movement.
        HexCoord destination = ChooseDestination(card.Mode, hostile, primary, active, card.Movement);
        if (destination != hostile.Position)
        {
            _resolver.MoveTo(hostile, destination);
        }

        // Actions.
        foreach (ActionEffect effect in card.Actions)
        {
            ResolveEnemyAction(hostile, effect, card.Target, active);
        }
    }

    private void ResolveEnemyAction(Hostile hostile, ActionEffect effect, TargetPriority priority, IReadOnlyList<Marine> active)
    {
        switch (effect.Kind)
        {
            case ActionKind.AttackMelee:
            {
                var eligible = active.Where(m => m.IsActive && IsMeleeReachable(hostile.Position, m.Position)).ToList();
                Marine? target = Rank(priority, hostile, eligible).FirstOrDefault();
                if (target is not null)
                {
                    Attack(hostile, target, effect);
                }

                break;
            }

            case ActionKind.AttackRanged:
            {
                var eligible = active.Where(m => m.IsActive && InRangeWithSight(hostile.Position, m.Position, effect.Range)).ToList();
                Marine? target = Rank(priority, hostile, eligible).FirstOrDefault();
                if (target is not null)
                {
                    Attack(hostile, target, effect);
                }

                break;
            }

            case ActionKind.AttackArea:
            {
                foreach (Marine m in active.Where(m => m.IsActive && InRangeWithSight(hostile.Position, m.Position, effect.Range)).ToList())
                {
                    Attack(hostile, m, effect);
                }

                break;
            }

            default:
                // Enemies only attack in the MVP; move/heal/door/etc. are ignored.
                break;
        }
    }

    private void Attack(Hostile hostile, Marine target, ActionEffect effect)
    {
        int bonus = hostile.PendingBonusDamage;
        hostile.PendingBonusDamage = 0;
        _resolver.ResolveAttack(hostile, target, effect.Value, effect.Condition, bonus);
    }

    private Marine? SelectTarget(TargetPriority priority, Hostile hostile, IReadOnlyList<Marine> active)
    {
        if (priority == TargetPriority.None)
        {
            return null;
        }

        IEnumerable<Marine> candidates = priority is TargetPriority.ClosestMarineLos or TargetPriority.HighestHpMarineLos
            ? active.Where(m => HasSight(hostile.Position, m.Position))
            : active;

        return Rank(priority, hostile, candidates.ToList()).FirstOrDefault();
    }

    private List<Marine> Rank(TargetPriority priority, Hostile hostile, IReadOnlyList<Marine> candidates)
    {
        int Dist(Marine m) => hostile.Position.DistanceTo(m.Position);

        IOrderedEnumerable<Marine> ordered = priority switch
        {
            TargetPriority.HighestHpMarineLos => candidates.OrderByDescending(m => m.Hp).ThenBy(Dist).ThenBy(m => m.Id, StringComparer.Ordinal),
            TargetPriority.LowestHpMarine => candidates.OrderBy(m => m.Hp).ThenBy(Dist).ThenBy(m => m.Id, StringComparer.Ordinal),
            _ => candidates.OrderBy(Dist).ThenBy(m => m.Hp).ThenBy(m => m.Id, StringComparer.Ordinal),
        };

        return ordered.ToList();
    }

    private HexCoord ChooseDestination(MovementMode mode, Hostile hostile, Marine? primary, IReadOnlyList<Marine> active, int budget)
    {
        HexCoord pos = hostile.Position;
        if (mode == MovementMode.Static || budget <= 0)
        {
            return pos;
        }

        Dictionary<HexCoord, int> reachable = Movement.ReachableWithin(_board, pos, budget);

        switch (mode)
        {
            case MovementMode.ApproachTarget:
            {
                HexCoord goal = (primary ?? Nearest(active, pos))?.Position ?? pos;
                return Movement.BestCell(reachable, c => (c.DistanceTo(goal), reachable[c]), pos);
            }

            case MovementMode.FleeFromClosestMarine:
                return Movement.BestCell(reachable, c => (-MinDistanceToMarines(c, active), reachable[c]), pos);

            case MovementMode.RepositionForLos:
            {
                Marine? focus = primary ?? Nearest(active, pos);
                if (focus is null)
                {
                    return pos;
                }

                var withSight = reachable.Keys.Where(c => HasSight(c, focus.Position)).ToList();
                if (withSight.Count > 0)
                {
                    // Prefer a sighted cell that requires the fewest steps, staying at range.
                    return withSight
                        .OrderBy(c => reachable[c])
                        .ThenByDescending(c => c.DistanceTo(focus.Position))
                        .ThenBy(c => c.Q).ThenBy(c => c.R)
                        .First();
                }

                // No line of sight available: close in to find it.
                return Movement.BestCell(reachable, c => (c.DistanceTo(focus.Position), reachable[c]), pos);
            }

            case MovementMode.RepositionNoLos:
            {
                var hidden = reachable.Keys.Where(c => active.All(m => !HasSight(c, m.Position))).ToList();
                return hidden.Count == 0
                    ? pos
                    : hidden.OrderBy(c => reachable[c]).ThenBy(c => c.Q).ThenBy(c => c.R).First();
            }

            default:
                return pos;
        }
    }

    private static Marine? Nearest(IReadOnlyList<Marine> marines, HexCoord from) =>
        marines.OrderBy(m => from.DistanceTo(m.Position)).ThenBy(m => m.Id, StringComparer.Ordinal).FirstOrDefault();

    private static int MinDistanceToMarines(HexCoord cell, IReadOnlyList<Marine> marines) =>
        marines.Count == 0 ? 0 : marines.Min(m => cell.DistanceTo(m.Position));

    private bool HasSight(HexCoord a, HexCoord b) => LineOfSight.HasLineOfSight(_board, a, b);

    private bool InRangeWithSight(HexCoord from, HexCoord to, int range) =>
        from.DistanceTo(to) <= range && HasSight(from, to);

    private bool IsMeleeReachable(HexCoord from, HexCoord to) =>
        from.DistanceTo(to) == 1 && !_board.BlocksMovement(from, to);
}
