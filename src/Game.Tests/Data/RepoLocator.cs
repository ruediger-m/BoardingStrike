namespace BoardingStrike.Game.Tests.Data;

/// <summary>
/// Locates repo-relative paths from within the test run by walking up from the
/// test assembly directory to the solution file. Used by tests that validate
/// the real committed content under <c>godot/data/</c>.
/// </summary>
internal static class RepoLocator
{
    public static string GodotDataDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "BoardingStrike.sln")))
            {
                return Path.Combine(dir.FullName, "godot", "data");
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repo root (BoardingStrike.sln) above " + AppContext.BaseDirectory);
    }
}
