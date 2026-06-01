using BoardingStrike.Game.Cards;

namespace BoardingStrike.Game.Units;

/// <summary>
/// The set of status conditions currently on a unit. Semantics (docs/design/combat.md)
/// are applied by the round resolver:
/// <list type="bullet">
///   <item><b>Wounded</b> — 1 damage at the start of the unit's turn; cleared by any heal.</item>
///   <item><b>Immobilized</b> — blocks Move primitives; cleared at end of the round it was applied.</item>
///   <item><b>Stunned</b> — the unit skips its next turn, then the condition clears.
///   (We use "lose your next turn" semantics, which is the intent of the design doc's
///   "cannot ... its next turn"; this is the one place we resolve the doc's slightly
///   ambiguous "removed at end of round" wording in favour of the gameplay intent.)</item>
/// </list>
/// </summary>
public sealed class ConditionSet
{
    private readonly HashSet<ConditionKind> _active = [];

    public bool Has(ConditionKind kind) => _active.Contains(kind);

    public bool Add(ConditionKind kind) => _active.Add(kind);

    public bool Remove(ConditionKind kind) => _active.Remove(kind);

    public IReadOnlyCollection<ConditionKind> Active => _active;
}
