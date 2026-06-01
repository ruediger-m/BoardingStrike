using BoardingStrike.Core.Rng;
using Xunit;

namespace BoardingStrike.Game.Tests.Engine;

public sealed class DeterministicRngTests
{
    [Fact]
    public void Same_seed_yields_identical_sequences()
    {
        var a = DeterministicRng.FromString("mission_hangar_sweep");
        var b = DeterministicRng.FromString("mission_hangar_sweep");

        for (int i = 0; i < 50; i++)
        {
            Assert.Equal(a.NextInt(1000), b.NextInt(1000));
        }
    }

    [Fact]
    public void Different_seeds_diverge()
    {
        var a = DeterministicRng.FromString("alpha");
        var b = DeterministicRng.FromString("beta");

        bool anyDifferent = false;
        for (int i = 0; i < 20; i++)
        {
            if (a.NextInt(1_000_000) != b.NextInt(1_000_000))
            {
                anyDifferent = true;
            }
        }

        Assert.True(anyDifferent);
    }

    [Fact]
    public void NextInt_is_in_range()
    {
        var rng = new DeterministicRng(12345);
        for (int i = 0; i < 1000; i++)
        {
            int value = rng.NextInt(6);
            Assert.InRange(value, 0, 5);
        }
    }

    [Fact]
    public void Shuffle_is_deterministic_for_the_same_seed()
    {
        var listA = Enumerable.Range(0, 20).ToList();
        var listB = Enumerable.Range(0, 20).ToList();
        new DeterministicRng(99).Shuffle(listA);
        new DeterministicRng(99).Shuffle(listB);

        Assert.Equal(listA, listB);
    }
}
