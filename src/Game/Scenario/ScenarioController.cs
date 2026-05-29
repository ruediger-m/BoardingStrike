using BoardingStrike.Core;

namespace BoardingStrike.Game.Scenario;

/// <summary>
/// Public entry point the presentation layer talks to. In the finished design
/// (see docs/technical/architecture.md) this owns the ScenarioState, runs the
/// RoundResolver, and emits an event stream that the renderer animates.
///
/// Iteration 1, Step 1 (skeleton): this is a stub. It exists only to prove the
/// Presentation -> Game -> Core dependency boundary is wired correctly and that
/// no Godot type leaks into the gameplay assemblies. It is replaced with the
/// real controller in Step 5.
/// </summary>
public sealed class ScenarioController
{
    /// <summary>
    /// A sign-of-life string the presentation/test layer can surface to confirm
    /// the Game layer is reachable and is itself reaching into Core.
    /// </summary>
    public string Status { get; }

    /// <summary>Number of times <see cref="Ping"/> has been called.</summary>
    public int PingCount { get; private set; }

    public ScenarioController()
    {
        // Reaching into Core here proves the Game -> Core reference resolves.
        Status = $"Game layer online — {BuildInfo.Banner}";
    }

    /// <summary>
    /// Stand-in for the real "advance the scenario" call. For the skeleton it
    /// just increments a counter and echoes back the status, so a caller can
    /// verify a round-trip through the boundary.
    /// </summary>
    public string Ping()
    {
        PingCount++;
        return $"{Status} (ping {PingCount})";
    }
}
