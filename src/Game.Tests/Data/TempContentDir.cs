namespace BoardingStrike.Game.Tests.Data;

/// <summary>
/// Builds a throwaway content directory tree on disk for loader tests. Files are
/// written into category subfolders; the whole tree is deleted on Dispose. This
/// keeps each test hermetic and makes "bad content" cases trivial to express.
/// </summary>
internal sealed class TempContentDir : IDisposable
{
    public TempContentDir()
    {
        Root = Path.Combine(Path.GetTempPath(), "bstrike_content_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    /// <summary>Writes a file under the given category subfolder, creating it as needed.</summary>
    public TempContentDir Write(string category, string fileName, string json)
    {
        string dir = Path.Combine(Root, category);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, fileName), json);
        return this;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup; a leaked temp dir must never fail a test.
        }
    }
}
