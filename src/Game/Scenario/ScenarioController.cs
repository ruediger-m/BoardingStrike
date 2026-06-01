using BoardingStrike.Game.Board;
using BoardingStrike.Game.Content;
using BoardingStrike.Game.Events;
using BoardingStrike.Game.Units;

namespace BoardingStrike.Game.Scenario;

/// <summary>
/// The public entry point the presentation layer (and tests) talk to. Owns the
/// <see cref="ScenarioState"/> and drives it one round at a time via
/// <see cref="RoundResolver"/>. Replaces the Step-1 stub.
/// </summary>
public sealed class ScenarioController : IScenarioView
{
    private readonly ScenarioState _state;
    private readonly RoundResolver _resolver;

    private ScenarioController(ScenarioState state)
    {
        _state = state;
        _resolver = new RoundResolver(state, this);
    }

    public int Round => _state.Round;

    public ScenarioStatus Status => _state.Status;

    public BoardState Board => _state.Board;

    public IReadOnlyList<Marine> Marines => _state.Marines;

    public IReadOnlyList<Hostile> Hostiles => _state.Hostiles;

    public string MissionName => _state.Mission.Name;

    /// <summary>
    /// Starts a scenario from loaded content. Optionally override the RNG seed
    /// and the class used to fill marine slots (the MVP squad is all
    /// Boarding Marines).
    /// </summary>
    public static ScenarioController Start(
        ContentCatalog catalog, string missionId, ulong? seedOverride = null, string marineClassId = "boarding_marine") =>
        new(ScenarioState.Build(catalog, missionId, seedOverride, marineClassId));

    /// <summary>Plays one full round, returning the events that occurred.</summary>
    public IReadOnlyList<GameEvent> PlayRound(IMarineController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return _resolver.PlayRound(controller);
    }

    /// <summary>
    /// Plays rounds until the scenario ends or <paramref name="maxRounds"/> is
    /// reached, returning the concatenated event stream. Useful for headless
    /// runs and tests.
    /// </summary>
    public IReadOnlyList<GameEvent> Play(IMarineController controller, int maxRounds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var all = new List<GameEvent>();
        for (int i = 0; i < maxRounds && Status == ScenarioStatus.InProgress; i++)
        {
            all.AddRange(_resolver.PlayRound(controller));
        }

        return all;
    }
}
