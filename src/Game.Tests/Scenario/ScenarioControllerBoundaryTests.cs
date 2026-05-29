using BoardingStrike.Core;
using BoardingStrike.Game.Scenario;
using Xunit;

namespace BoardingStrike.Game.Tests.Scenario;

/// <summary>
/// Skeleton boundary test (Iteration 1, Step 1). Confirms that:
///   - the test project can reach the Game layer,
///   - the Game layer can reach Core,
///   - a round-trip call through the stub controller behaves.
/// This is the "one passing test exists" exit criterion for Step 1.
/// </summary>
public sealed class ScenarioControllerBoundaryTests
{
    [Fact]
    public void Status_includes_core_banner()
    {
        var controller = new ScenarioController();

        Assert.Contains(BuildInfo.Banner, controller.Status);
    }

    [Fact]
    public void Ping_increments_and_echoes_status()
    {
        var controller = new ScenarioController();

        var first = controller.Ping();
        var second = controller.Ping();

        Assert.Equal(2, controller.PingCount);
        Assert.Contains("ping 1", first);
        Assert.Contains("ping 2", second);
        Assert.Contains(BuildInfo.Product, second);
    }
}
