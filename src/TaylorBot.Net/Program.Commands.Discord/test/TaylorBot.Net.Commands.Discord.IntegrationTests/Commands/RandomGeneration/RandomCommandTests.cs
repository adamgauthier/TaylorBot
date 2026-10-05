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
            arguments: [SlashArgument.Text("options", " , Fearless, Speak Now, , Red, Fearless, ")]);

        response.Description.Should().StartWith("## ");
        response.Description[3..].Should().BeOneOf("Fearless", "Speak Now", "Red");
        var field = response.Embed.GetProperty("fields").EnumerateArray().Should().ContainSingle().Which;
        field.GetProperty("name").GetString().Should().Contain("4");
        field.GetProperty("value").GetString()!.Split(", ", StringSplitOptions.None)
            .Should().Equal("Fearless", "Speak Now", "Red", "Fearless");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(30)]
    public async Task Choose_DisplaysParsedOptions(int count)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var options = Enumerable.Range(1, count).Select(number => $"Option {number}").ToArray();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "choose",
            arguments: [SlashArgument.Text("options", string.Join(", ", options))]);

        response.ShouldBeSuccess();
        response.Description[3..].Should().BeOneOf(options);
        var field = response.Embed.GetProperty("fields").EnumerateArray().Should().ContainSingle().Which;
        field.GetProperty("name").GetString().Should().Contain(count.ToString(CultureInfo.InvariantCulture));
        field.GetProperty("value").GetString()!.Split(", ", StringSplitOptions.None).Should().Equal(options);
    }

    [Theory]
    [InlineData(1023)]
    [InlineData(1024)]
    [InlineData(1025)]
    public async Task Choose_OptionsStayWithinDiscordFieldLimit(int length)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        string option = new('A', count: length - 3);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "choose",
            arguments: [SlashArgument.Text("options", $"{option}, B")]);

        response.ShouldBeSuccess();
        response.Description[3..].Should().BeOneOf(option, "B");
        var field = response.Embed.GetProperty("fields").EnumerateArray().Should().ContainSingle().Which;
        field.GetProperty("value").GetString().Should().HaveLength(Math.Min(length, val2: 1024));
    }

    [Theory]
    [InlineData("**pizza**", @"\*\*pizza\*\*")]
    [InlineData("||sushi||", @"\|\|sushi\|\|")]
    [InlineData("`ramen`", @"\`ramen\`")]
    [InlineData("Pizza\r\n# Fake heading", @"Pizza \# Fake heading")]
    [InlineData("Pizza\n> Fake quote", @"Pizza \> Fake quote")]
    [InlineData("[Dinner](https://example.com)", @"\[Dinner](https\:\/\/example\.com)")]
    [InlineData("- Dinner", @"\- Dinner")]
    [InlineData("<@123456789012345678>", @"\<@123456789012345678\>")]
    [InlineData(@"\**Dinner**", @"\\\*\*Dinner\*\*")]
    public async Task Choose_DisplaysUserFormattingAsLiteralText(string option, string displayedOption)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "choose",
            arguments: [SlashArgument.Text("options", option)]);

        response.ShouldBeSuccess();
        response.Description.Should().Be($"## {displayedOption}");
        var field = response.Embed.GetProperty("fields").EnumerateArray().Should().ContainSingle().Which;
        field.GetProperty("value").GetString().Should().Be(displayedOption);
    }

    [Theory]
    [InlineData('A', 4093)]
    [InlineData('A', 4094)]
    [InlineData('A', 6000)]
    [InlineData('*', 2046)]
    [InlineData('*', 2047)]
    [InlineData('*', 6000)]
    public async Task Choose_HeadingAndEscapingStayWithinDiscordLimits(char character, int length)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        string option = new(character, length);
        var displayedLength = character == '*' ? length * 2 : length;

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "choose",
            arguments: [SlashArgument.Text("options", option)]);

        response.ShouldBeSuccess();
        response.Description.Should().StartWith("## ").And.HaveLength(Math.Min(displayedLength + 3, val2: 4096));
        var field = response.Embed.GetProperty("fields").EnumerateArray().Should().ContainSingle().Which;
        field.GetProperty("value").GetString().Should().HaveLength(Math.Min(displayedLength, val2: 1024));
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

    [Theory]
    [InlineData("!choose Fearless, Speak Now, Red")]
    [InlineData("!choice Fearless, Speak Now, Red")]
    [InlineData("!choose")]
    [InlineData("!choice")]
    public async Task LegacyChoose_RedirectsToSlashCommand(string message)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, message);

        response.Description.Should().Contain("has been moved").And.Contain("/choose");
    }
}
