using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

public sealed class DisabledCommandTests(DataServices data)
{
    [Fact]
    public async Task DisabledCommand_RejectsRequestsAndCachesReason()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 13);
        await scenario.Given.DisableCommandAsync("taypoints", "Temporarily unavailable.");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        response.ShouldHaveSingleEmbedDescription().Should()
            .Contain("</taypoints:")
            .And.Contain("globally disabled")
            .And.Contain("Temporarily unavailable.");
        (await scenario.State.CachedDisabledReasonAsync("taypoints")).Should().Be("Temporarily unavailable.");
    }

    [Fact]
    public async Task DisabledCommand_CacheHitDoesNotReadChangedDatabaseReason()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync(taypoints: 13);
        await scenario.Given.DisableCommandAsync("taypoints", "Cached reason.");
        await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");
        await scenario.Given.DisableCommandAsync("taypoints", "Changed in storage.");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "taypoints balance");

        response.ShouldHaveSingleEmbedDescription().Should()
            .Contain("globally disabled")
            .And.Contain("Cached reason.")
            .And.NotContain("Changed in storage.");
    }
}
