namespace BoardingStrike.Core.Data;

/// <summary>
/// Thrown by <see cref="JsonContentLoader.Load"/> when content fails to load or
/// validate. Carries <em>all</em> discovered problems (not just the first) so an
/// author can fix everything in one pass. The message lists each error on its
/// own line.
/// </summary>
public sealed class ContentValidationException : Exception
{
    public ContentValidationException(IReadOnlyList<string> errors)
        : base(BuildMessage(errors))
    {
        Errors = errors;
    }

    /// <summary>Every validation error, in deterministic order.</summary>
    public IReadOnlyList<string> Errors { get; }

    private static string BuildMessage(IReadOnlyList<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return $"Content validation failed with {errors.Count} error(s):"
            + Environment.NewLine
            + string.Join(Environment.NewLine, errors.Select(e => "  - " + e));
    }
}
