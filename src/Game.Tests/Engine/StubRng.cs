using BoardingStrike.Core.Rng;

namespace BoardingStrike.Game.Tests.Engine;

/// <summary>
/// A deterministic test RNG. <see cref="Shuffle"/> is a no-op so collection
/// order is preserved (handy for asserting modifier-deck draw order), and
/// <see cref="NextInt"/> returns scripted values (or 0).
/// </summary>
internal sealed class StubRng : IRandomSource
{
    private readonly Queue<int> _values;

    public StubRng(params int[] values) => _values = new Queue<int>(values);

    public int NextInt(int maxExclusive) => _values.Count > 0 ? _values.Dequeue() % maxExclusive : 0;

    public void Shuffle<T>(IList<T> list)
    {
        // Intentionally no-op: preserve insertion order for predictable tests.
    }
}
