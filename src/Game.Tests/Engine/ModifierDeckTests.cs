using BoardingStrike.Game.Combat;
using Xunit;

namespace BoardingStrike.Game.Tests.Engine;

public sealed class ModifierDeckTests
{
    [Theory]
    [InlineData(ModifierEffectKind.Add, 1, 3, 4)]
    [InlineData(ModifierEffectKind.Add, -2, 1, -1)]
    [InlineData(ModifierEffectKind.Multiply, 0, 3, 0)]
    [InlineData(ModifierEffectKind.Multiply, 2, 3, 6)]
    public void Apply_computes_modified_damage(ModifierEffectKind effect, int value, int baseDamage, int expected)
    {
        Assert.Equal(expected, new ModifierCard(effect, value, false).Apply(baseDamage));
    }

    [Fact]
    public void Rolling_card_triggers_reshuffle_after_the_draw()
    {
        ModifierCard[] cards =
        [
            new(ModifierEffectKind.Add, 1, false),
            new(ModifierEffectKind.Multiply, 2, true), // ×2 reshuffles
            new(ModifierEffectKind.Add, 2, false),
        ];
        var deck = new ModifierDeck(cards, new StubRng()); // no-op shuffle -> insertion order

        ModifierCard first = deck.Draw();
        ModifierCard second = deck.Draw();
        ModifierCard third = deck.Draw();

        Assert.Equal(1, first.Value);
        Assert.True(second.Reshuffle);
        // The reshuffle reset the pile, so the third draw is the first card again.
        Assert.Equal(1, third.Value);
    }

    [Fact]
    public void Deck_cycles_through_all_cards_then_reshuffles()
    {
        ModifierCard[] cards =
        [
            new(ModifierEffectKind.Add, 0, false),
            new(ModifierEffectKind.Add, 1, false),
            new(ModifierEffectKind.Add, -1, false),
        ];
        var deck = new ModifierDeck(cards, new StubRng());

        Assert.Equal(0, deck.Draw().Value);
        Assert.Equal(1, deck.Draw().Value);
        Assert.Equal(-1, deck.Draw().Value);
        // Pile exhausted -> reshuffle on next draw.
        Assert.Equal(0, deck.Draw().Value);
    }
}
