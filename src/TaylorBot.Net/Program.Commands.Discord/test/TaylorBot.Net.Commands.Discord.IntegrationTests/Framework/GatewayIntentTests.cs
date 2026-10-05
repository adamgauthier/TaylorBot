using Discord;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Framework;

public sealed class GatewayIntentTests(DataServices data)
{
    [Fact]
    public async Task Startup_RequestsMessagesButNotReactions()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);

        var intents = (GatewayIntents)scenario.RequestedGatewayIntents;

        intents.Should().HaveFlag(GatewayIntents.GuildMessages).And.HaveFlag(GatewayIntents.DirectMessages);
        intents.Should().NotHaveFlag(GatewayIntents.GuildMessageReactions).And.NotHaveFlag(GatewayIntents.DirectMessageReactions);
    }
}
