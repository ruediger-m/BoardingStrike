using BoardingStrike.Game.Content;
using BoardingStrike.Game.Events;
using BoardingStrike.Game.Scenario;
using BoardingStrike.Game.Tests.Data;
using BoardingStrike.Game.Units;
using Xunit;

namespace BoardingStrike.Game.Tests.Engine;

public sealed class ScenarioEngineTests
{
    [Fact]
    public void Scenario_starts_with_the_expected_squad_and_hostiles()
    {
        ContentCatalog catalog = SyntheticContent.BuildArena();
        ScenarioController scenario = ScenarioController.Start(catalog, "test_clear", marineClassId: "test_marine");

        Assert.Equal(ScenarioStatus.InProgress, scenario.Status);
        Assert.Single(scenario.Marines);
        Assert.Single(scenario.Hostiles);
    }

    [Fact]
    public void Marine_kills_adjacent_hostile_and_wins()
    {
        ContentCatalog catalog = SyntheticContent.BuildArena();
        ScenarioController scenario = ScenarioController.Start(catalog, "test_clear", marineClassId: "test_marine");
        var controller = new AttackNearestController();

        IReadOnlyList<GameEvent> events = scenario.PlayRound(controller);

        Assert.Equal(ScenarioStatus.Victory, scenario.Status);
        Assert.False(scenario.Hostiles[0].IsAlive);
        Assert.Contains(events, e => e is AttackResolved { Killed: true });
        Assert.Contains(events, e => e is ScenarioEnded { Status: "Victory" });
    }

    [Fact]
    public void Marine_that_never_fights_exhausts_and_loses()
    {
        ContentCatalog catalog = SyntheticContent.BuildArena();
        ScenarioController scenario = ScenarioController.Start(catalog, "test_clear", marineClassId: "test_marine");
        var controller = new PassiveController();

        scenario.Play(controller, maxRounds: 10);

        Assert.Equal(ScenarioStatus.Failure, scenario.Status);
        Assert.True(scenario.Hostiles[0].IsAlive); // the dummy never died
        Assert.False(scenario.Marines[0].IsActive);
    }

    [Fact]
    public void Hangar_sweep_runs_to_a_terminal_state()
    {
        ContentCatalog catalog = ContentCatalog.Load(RepoLocator.GodotDataDir());
        ScenarioController scenario = ScenarioController.Start(catalog, "mission_hangar_sweep", seedOverride: 42);

        IReadOnlyList<GameEvent> events = scenario.Play(new AutoMarineController(), maxRounds: 80);

        // Reached a terminal state without errors...
        Assert.NotEqual(ScenarioStatus.InProgress, scenario.Status);
        // ...and the run was meaningful: the squad breached out of the airlock
        // and engaged (a door opened and at least one hostile took damage).
        Assert.Contains(events, e => e is DoorChanged { Open: true });
        Assert.Contains(events, e => e is AttackResolved a && a.DealtDamage > 0);
        Assert.True(scenario.Hostiles.Count(h => h.IsAlive) < scenario.Hostiles.Count, "no hostile was defeated");
    }

    [Fact]
    public void Hangar_sweep_is_deterministic_for_a_fixed_seed()
    {
        ContentCatalog catalog = ContentCatalog.Load(RepoLocator.GodotDataDir());

        ScenarioController a = ScenarioController.Start(catalog, "mission_hangar_sweep", seedOverride: 7);
        a.Play(new AutoMarineController(), maxRounds: 80);

        ScenarioController b = ScenarioController.Start(catalog, "mission_hangar_sweep", seedOverride: 7);
        b.Play(new AutoMarineController(), maxRounds: 80);

        Assert.Equal(a.Status, b.Status);
        Assert.Equal(a.Round, b.Round);
        Assert.Equal(a.Hostiles.Count(h => h.IsAlive), b.Hostiles.Count(h => h.IsAlive));
    }

    private sealed class AttackNearestController : IMarineController
    {
        public MarineCommit Commit(Marine marine, IScenarioView view) => new PlayCommit("test_strike", "test_step");

        public MarinePlan PlanTurn(Marine marine, PlayCommit committed, IScenarioView view)
        {
            Hostile target = view.Hostiles.First(h => h.IsAlive);
            return new MarinePlan(
                "test_strike",
                "test_step",
                TopFirst: true,
                TopTargets: [EffectTarget.Unit(target.Id)],
                BottomTargets: [EffectTarget.None]);
        }
    }

    private sealed class PassiveController : IMarineController
    {
        public MarineCommit Commit(Marine marine, IScenarioView view) => new PlayCommit("test_strike", "test_step");

        public MarinePlan PlanTurn(Marine marine, PlayCommit committed, IScenarioView view) =>
            // Both halves get no targets, so nothing happens; cards just cycle to discard.
            new("test_strike", "test_step", TopFirst: true, TopTargets: [EffectTarget.None], BottomTargets: [EffectTarget.None]);
    }
}
