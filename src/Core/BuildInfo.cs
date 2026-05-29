namespace BoardingStrike.Core;

/// <summary>
/// Static build/version information for the engine-independent core.
/// Placeholder for the skeleton (Iteration 1, Step 1): exists so dependent
/// layers can reference Core and surface a sign-of-life string. Real version
/// wiring (assembly attributes, semantic version) lands when it is needed.
/// </summary>
public static class BuildInfo
{
    /// <summary>Human-readable product name.</summary>
    public const string Product = "Boarding Strike";

    /// <summary>Pre-release version of the in-progress build.</summary>
    public const string Version = "0.1.0-dev";

    /// <summary>Short banner used by sign-of-life checks across layers.</summary>
    public static string Banner => $"{Product} {Version}";
}
