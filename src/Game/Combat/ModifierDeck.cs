using BoardingStrike.Core.Rng;

namespace BoardingStrike.Game.Combat;

public enum ModifierEffectKind
{
    Multiply,
    Add,
}

/// <summary>One attack-modifier card (docs/design/combat.md).</summary>
public readonly record struct ModifierCard(ModifierEffectKind Effect, int Value, bool Reshuffle)
{
    /// <summary>Applies this modifier to a base damage value.</summary>
    public int Apply(int baseDamage) =>
        Effect == ModifierEffectKind.Multiply ? baseDamage * Value : baseDamage + Value;
}

/// <summary>The immutable composition of a modifier deck; spawns live decks.</summary>
public sealed class ModifierDeckTemplate
{
    public ModifierDeckTemplate(IReadOnlyList<ModifierCard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        if (cards.Count == 0)
        {
            throw new ArgumentException("A modifier deck needs at least one card.", nameof(cards));
        }

        Cards = cards;
    }

    public IReadOnlyList<ModifierCard> Cards { get; }

    public ModifierDeck CreateDeck(IRandomSource rng) => new(Cards, rng);
}

/// <summary>
/// A live, shuffled modifier deck. Drawing a ×0 or ×2 ("rolling") card triggers
/// a reshuffle after the attack, per docs/design/combat.md.
/// </summary>
public sealed class ModifierDeck
{
    private readonly List<ModifierCard> _multiset;
    private readonly IRandomSource _rng;
    private List<ModifierCard> _order = [];
    private int _next;

    public ModifierDeck(IReadOnlyList<ModifierCard> cards, IRandomSource rng)
    {
        ArgumentNullException.ThrowIfNull(cards);
        ArgumentNullException.ThrowIfNull(rng);
        _multiset = [.. cards];
        _rng = rng;
        Reshuffle();
    }

    public ModifierCard Draw()
    {
        if (_next >= _order.Count)
        {
            Reshuffle();
        }

        ModifierCard card = _order[_next++];
        if (card.Reshuffle)
        {
            Reshuffle();
        }

        return card;
    }

    private void Reshuffle()
    {
        _order = [.. _multiset];
        _rng.Shuffle(_order);
        _next = 0;
    }
}
