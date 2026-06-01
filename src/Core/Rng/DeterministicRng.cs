namespace BoardingStrike.Core.Rng;

/// <summary>
/// A small, self-contained pseudo-random generator (SplitMix64). Chosen over
/// <see cref="System.Random"/> because its algorithm is fixed here in source, so
/// the same seed yields the same sequence on every platform and .NET version —
/// essential for reproducible scenarios, tests, and future replays.
/// </summary>
public sealed class DeterministicRng : IRandomSource
{
    private ulong _state;

    public DeterministicRng(ulong seed) => _state = seed;

    /// <summary>
    /// Derives a seed from an arbitrary string (e.g. a scenario id) via FNV-1a,
    /// so <c>rng_seed_source: "scenario_id"</c> is stable and content-defined.
    /// </summary>
    public static DeterministicRng FromString(string seed)
    {
        ArgumentNullException.ThrowIfNull(seed);
        ulong hash = 1469598103934665603UL; // FNV offset basis
        foreach (char c in seed)
        {
            hash ^= c;
            hash *= 1099511628211UL; // FNV prime
        }

        // Avoid a degenerate all-zero state.
        return new DeterministicRng(hash == 0 ? 0x9E3779B97F4A7C15UL : hash);
    }

    private ulong NextUInt64()
    {
        _state += 0x9E3779B97F4A7C15UL;
        ulong z = _state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    public int NextInt(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Must be positive.");
        }

        return (int)(NextUInt64() % (ulong)maxExclusive);
    }

    public void Shuffle<T>(IList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = NextInt(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
