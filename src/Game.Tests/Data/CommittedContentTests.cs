using BoardingStrike.Core.Data;
using Xunit;

namespace BoardingStrike.Game.Tests.Data;

/// <summary>
/// Guards that the content actually committed under <c>godot/data/</c> always
/// loads and validates. As Step 4 authors the full MVP content, this test keeps
/// it honest in CI.
/// </summary>
public sealed class CommittedContentTests
{
    [Fact]
    public void CommittedContentValidates()
    {
        string dataDir = LocateGodotDataDir();

        bool ok = JsonContentLoader.TryLoad(dataDir, out ContentDatabase? db, out IReadOnlyList<string> errors);

        Assert.True(ok, "Committed content failed validation:" + Environment.NewLine + string.Join(Environment.NewLine, errors));
        Assert.NotNull(db);

        // Sanity: the two seeded modifier decks are present.
        Assert.True(db!.ModifierDecks.ContainsKey("standard_marine"));
        Assert.True(db.ModifierDecks.ContainsKey("standard_hostile"));
    }

    private static string LocateGodotDataDir()
    {
        // Walk up from the test assembly location until we find the repo root
        // (identified by the solution file), then resolve godot/data.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "BoardingStrike.sln")))
            {
                string dataDir = Path.Combine(dir.FullName, "godot", "data");
                Assert.True(Directory.Exists(dataDir), $"Expected content directory not found: {dataDir}");
                return dataDir;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repo root (BoardingStrike.sln) above " + AppContext.BaseDirectory);
    }
}
