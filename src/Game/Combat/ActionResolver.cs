using BoardingStrike.Core.Hex;
using BoardingStrike.Core.Rng;
using BoardingStrike.Game.Board;
using BoardingStrike.Game.Cards;
using BoardingStrike.Game.Events;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Combat;

/// <summary>
/// Applies the low-level effects shared by marine and enemy turns — attacks
/// (with the modifier deck), moves, heals, conditions, and doors — mutating the
/// board/units and appending events. Resolution order follows
/// docs/design/combat.md. Used by both the marine turn executor and the enemy
/// AI executor.
/// </summary>
public sealed class ActionResolver
{
    private readonly BoardState _board;
    private readonly IList<GameEvent> _log;

    public ActionResolver(BoardState board, IRandomSource rng, IList<GameEvent> log)
    {
        _board = board;
        _ = rng; // reserved for future randomized effects beyond the modifier deck
        _log = log;
    }

    /// <summary>
    /// Resolves a single attack from <paramref name="attacker"/> against
    /// <paramref name="target"/>: draw a modifier, apply it to base (+ any
    /// queued bonus), deal damage, then apply on-hit conditions only if damage
    /// landed. A killed target is removed before conditions would apply.
    /// </summary>
    public void ResolveAttack(Unit attacker, Unit target, int baseDamage, ConditionKind? onHit, int bonusDamage = 0)
    {
        ModifierCard modifier = attacker.Modifiers.Draw();
        int modified = Math.Max(0, modifier.Apply(baseDamage + bonusDamage));
        int dealt = target.TakeDamage(modified);
        bool killed = !target.IsAlive;

        _log.Add(new AttackResolved(attacker.Id, target.Id, baseDamage, modified, dealt, killed));

        if (killed)
        {
            KillUnit(target);
            return;
        }

        if (dealt > 0 && onHit is ConditionKind condition && target.Conditions.Add(condition))
        {
            _log.Add(new ConditionApplied(target.Id, condition));
        }
    }

    /// <summary>Relocates a unit to <paramref name="destination"/> (no path check; caller validates).</summary>
    public void MoveTo(Unit unit, HexCoord destination)
    {
        if (destination == unit.Position)
        {
            return;
        }

        HexCoord from = unit.Position;
        _board.MoveUnit(unit, destination);
        _log.Add(new UnitMoved(unit.Id, from, destination));
    }

    public void Heal(Unit unit, int amount)
    {
        bool hadWound = unit.Conditions.Has(ConditionKind.Wounded);
        unit.Heal(amount);
        _log.Add(new HealApplied(unit.Id, amount));
        if (hadWound)
        {
            _log.Add(new ConditionExpired(unit.Id, ConditionKind.Wounded));
        }
    }

    public void ApplyCondition(Unit unit, ConditionKind condition)
    {
        if (unit.Conditions.Add(condition))
        {
            _log.Add(new ConditionApplied(unit.Id, condition));
        }
    }

    public void SetDoor(HexEdge edge, bool open)
    {
        _board.SetDoor(edge, open);
        _log.Add(new DoorChanged(edge, open));
    }

    public void ToggleDoor(HexEdge edge)
    {
        bool open = _board.ToggleDoor(edge);
        _log.Add(new DoorChanged(edge, open));
    }

    private void KillUnit(Unit unit)
    {
        _board.RemoveUnit(unit);
        if (unit is Marine marine)
        {
            marine.IsExhausted = true;
            _log.Add(new MarineExhausted(marine.Id));
        }
    }
}
