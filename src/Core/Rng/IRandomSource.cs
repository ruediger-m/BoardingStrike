namespace BoardingStrike.Core.Rng;

/// <summary>
/// A source of deterministic randomness for the rules engine. All in-game
/// randomness (modifier-deck shuffles, AI-deck draws, refresh burns) draws from
/// one of these, seeded per scenario, so a run is fully reproducible
/// (docs/technical/architecture.md "Determinism").
/// </summary>
public interface IRandomSource
{
    /// <summary>A uniform integer in [0, maxExclusive).</summary>
    int NextInt(int maxExclusive);

    /// <summary>In-place Fisher–Yates shuffle.</summary>
    void Shuffle<T>(IList<T> list);
}
