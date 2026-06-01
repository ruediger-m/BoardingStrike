using BoardingStrike.Core.Rng;
using BoardingStrike.Game.Content;

namespace BoardingStrike.Game.Scenario;

/// <summary>
/// One enemy type's AI deck as a draw pile: one card is drawn per round for the
/// whole type (docs/design/core-loop.md). Sequential draw, reshuffled when the
/// pile is exhausted, using the scenario RNG for reproducibility.
/// </summary>
public sealed class AiDeckPile
{
    private readonly IReadOnlyList<AiCard> _cards;
    private readonly IRandomSource _rng;
    private List<AiCard> _order = [];
    private int _next;

    public AiDeckPile(IReadOnlyList<AiCard> cards, IRandomSource rng)
    {
        _cards = cards;
        _rng = rng;
        Reshuffle();
    }

    public AiCard Draw()
    {
        if (_next >= _order.Count)
        {
            Reshuffle();
        }

        return _order[_next++];
    }

    private void Reshuffle()
    {
        _order = [.. _cards];
        _rng.Shuffle(_order);
        _next = 0;
    }
}
