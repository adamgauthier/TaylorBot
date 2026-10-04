using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Server;

[Trait("Command", "server population")]
public sealed class PopulationTests(DataServices data)
{
    [Fact]
    public async Task NoAttributes_ShowsNoDataWithoutPercentages()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server population", guild);

        response.Field("Age").Should().Contain("No Data");
        response.Field("Gender").Should().NotContain("%");
    }

    [Fact]
    public async Task Attributes_ShowActualAgeAndGenderAggregates()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.PopulationAsync(guild);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "server population", guild);

        response.Field("Age").Should().Contain("22").And.Contain("15");
        response.Field("Gender").Should().Contain("3 (30.0%)");
    }
}
