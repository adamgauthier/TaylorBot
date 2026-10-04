using FluentAssertions;
using System.Globalization;
using System.Text.RegularExpressions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.RandomGeneration;

[Trait("Command", "choose")]
[Trait("Command", "dice")]
public sealed class RandomCommandTests(DataServices data)
{
    [Fact]
    public async Task Choose_ReturnsOneOfTheProvidedOptions()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "choose",
            arguments: [SlashArgument.Text("options", "Fearless, Speak Now, Red")]);

        response.Description.Split('\n')[0].Trim().Should().BeOneOf("Fearless", "Speak Now", "Red");
    }

    [Theory]
    [InlineData(",")]
    [InlineData("  ,  ")]
    [InlineData(", ,")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Choose_RejectsEmptyOptions(string options)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "choose", arguments: [SlashArgument.Text("options", options)]);

        response.Description.Should().Contain("Please provide at least one option");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    public async Task Dice_RollIsWithinRequestedFaces(int faces)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "dice", arguments: [SlashArgument.Integer("faces", faces)]);

        var number = Regex.Matches(response.Description, @"\d[\d,]*").Should().ContainSingle().Which.Value;
        int.Parse(number, NumberStyles.AllowThousands, CultureInfo.InvariantCulture).Should().BeInRange(1, faces);
    }

    [Fact]
    public async Task LegacyChoose_ParsesCommaSeparatedOptions()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!choose Fearless, Speak Now, Red");

        response.Description.Split('\n')[0].Trim().Should().BeOneOf("Fearless", "Speak Now", "Red");
    }
}
