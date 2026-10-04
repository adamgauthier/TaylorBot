using System.Text.Json;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;
using Xunit;
using Xunit.Sdk;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Taypoints;

public sealed class TaypointResponseAssertionsTests
{
    private static readonly ScenarioUser User = new("100000000000000003", "Alice");

    [Theory]
    [InlineData("<@100000000000000003> has **1,234** taypoints \U0001FA99\n")]
    [InlineData("<@100000000000000003> has 1234 taypoints")]
    [InlineData("<@100000000000000003> has **1234** taypoints \U0001F389")]
    public void BalanceAssertion_IgnoresPresentation(string description)
    {
        var response = CreateResponse(description);

        response.ShouldShowBalance(User, taypoints: 1_234);
    }

    [Theory]
    [InlineData("<@100000000000000004> has 1,234 taypoints")]
    [InlineData("<@100000000000000003> has 12,345 taypoints")]
    [InlineData("<@100000000000000003> has 1,234 taypoints\n1st in this server's leaderboard")]
    public void BalanceAssertion_RejectsWrongUserAmountOrRank(string description)
    {
        var response = CreateResponse(description);

        var assertion = () => response.ShouldShowBalance(User, taypoints: 1_234);

        assertion.Should().Throw<XunitException>();
    }

    private static DiscordExchange CreateResponse(string description) => new([
        new("POST", "interactions/synthetic/token/callback", JsonSerializer.SerializeToElement(new { type = 5 })),
        new("POST", $"webhooks/{DiscordApi.ApplicationId}/token",
            JsonSerializer.SerializeToElement(new { embeds = new[] { new { description } } })),
    ]);
}
