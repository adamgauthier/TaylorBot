using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Favorite;

[Trait("Command", "favorite songs show")]
[Trait("Command", "favorite songs set")]
[Trait("Command", "favorite songs clear")]
[Trait("Command", "favorite bae show")]
[Trait("Command", "favorite bae set")]
[Trait("Command", "favorite bae clear")]
[Trait("Command", "favorite obsession show")]
[Trait("Command", "favorite obsession set")]
[Trait("Command", "favorite obsession clear")]
public sealed class FavoriteTests(DataServices data)
{
    [Theory]
    [InlineData("songs", "songs", "favoritesongs", "  All Too Well, , Style  ", "All Too Well\nStyle")]
    [InlineData("bae", "bae", "bae", "Taylor Swift", "Taylor Swift")]
    [InlineData("obsession", "obsession", "waifu", "https://images.example.invalid/taylor.jpg", "https://images.example.invalid/taylor.jpg")]
    public async Task Set_ConfirmationPersistsPreview(string feature, string option, string attribute, string input, string expected)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, $"favorite {feature} set",
            arguments: [SlashArgument.Text(option, input)]);
        prompt.Message.GetProperty("embeds").GetArrayLength().Should().Be(2);
        (await scenario.State.ProfileTextAsync(user, attribute)).Should().BeNull();

        var response = await scenario.Discord.ClickAsync(user, prompt, "Confirm");

        response.ShouldBeSuccess();
        (await scenario.State.ProfileTextAsync(user, attribute)).Should().Be(expected);
    }

    [Theory]
    [InlineData("songs", "songs", "favoritesongs", "Style")]
    [InlineData("bae", "bae", "bae", "Taylor Swift")]
    [InlineData("obsession", "obsession", "waifu", "https://images.example.invalid/taylor.jpg")]
    public async Task Set_CancelPreservesExistingValue(string feature, string option, string attribute, string input)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.TextAttributeAsync(user, attribute, "Original");
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, $"favorite {feature} set",
            arguments: [SlashArgument.Text(option, input)]);

        var response = await scenario.Discord.ClickAsync(user, prompt, "Cancel");

        response.Description.Should().Contain("cancelled");
        (await scenario.State.ProfileTextAsync(user, attribute)).Should().Be("Original");
    }

    [Theory]
    [InlineData("songs", "songs", "favoritesongs", "Style")]
    [InlineData("bae", "bae", "bae", "Taylor Swift")]
    [InlineData("obsession", "obsession", "waifu", "https://images.example.invalid/taylor.jpg")]
    public async Task Set_AnotherUserCannotConfirm(string feature, string option, string attribute, string input)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        var prompt = await scenario.Discord.InvokeSlashCommandAsync(user, $"favorite {feature} set",
            arguments: [SlashArgument.Text(option, input)]);

        var response = await scenario.Discord.ClickAsync(other, prompt, "Confirm");

        response.Requests.Should().ContainSingle("another user's click is acknowledged without editing the original message");
        (await scenario.State.ProfileTextAsync(user, attribute)).Should().BeNull();
        (await scenario.State.ProfileTextAsync(other, attribute)).Should().BeNull();
    }

    [Theory]
    [InlineData("songs", "favoritesongs", "All Too Well\nStyle")]
    [InlineData("bae", "bae", "Taylor Swift")]
    public async Task Show_DisplaysSelectedUsersText(string feature, string attribute, string value)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        await scenario.Given.TextAttributeAsync(other, attribute, value);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"favorite {feature} show", selectedUser: other);

        response.ShouldBeSuccess();
        response.Description.Should().Be(value);
    }

    [Fact]
    public async Task ObsessionShow_DisplaysSelectedUsersImage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync(username: "Other");
        await scenario.Given.TextAttributeAsync(other, "waifu", "https://images.example.invalid/taylor.jpg");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "favorite obsession show", selectedUser: other);

        response.ShouldBeSuccess();
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("https://images.example.invalid/taylor.jpg");
    }

    [Theory]
    [InlineData("songs")]
    [InlineData("bae")]
    [InlineData("obsession")]
    public async Task Show_UnsetExplainsHowToSet(string feature)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"favorite {feature} show");

        response.ShouldBeError();
        response.Description.Should().Contain("not set").And.Contain($"favorite {feature} set");
    }

    [Theory]
    [InlineData("songs", "favoritesongs")]
    [InlineData("bae", "bae")]
    [InlineData("obsession", "waifu")]
    public async Task Clear_RemovesOnlyCallersValue(string feature, string attribute)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var other = await scenario.Given.UserAsync();
        await scenario.Given.TextAttributeAsync(user, attribute, "Original");
        await scenario.Given.TextAttributeAsync(other, attribute, "Unchanged");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, $"favorite {feature} clear");

        response.ShouldBeSuccess();
        (await scenario.State.ProfileTextAsync(user, attribute)).Should().BeNull();
        (await scenario.State.ProfileTextAsync(other, attribute)).Should().Be("Unchanged");
    }

    [Theory]
    [InlineData("not a URL")]
    [InlineData("file:///private/photo.jpg")]
    public async Task ObsessionSet_RejectsInvalidImageUrl(string value)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "favorite obsession set",
            arguments: [SlashArgument.Text("obsession", value)]);

        response.ShouldBeError();
        response.Description.Should().Contain("not a valid URL");
        (await scenario.State.ProfileTextAsync(user, "waifu")).Should().BeNull();
    }

    [Fact]
    public async Task ObsessionShow_InvalidHistoricalValueExplainsHowToRepair()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.TextAttributeAsync(user, "waifu", "historical non-image value");

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "favorite obsession show");

        response.ShouldBeError();
        response.Description.Should().Contain("not a valid URL").And.Contain("favorite obsession set");
    }

    [Fact]
    public async Task LegacySetSongs_PersistsUnsplitTextWithoutConfirmation()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!setfav Style, All Too Well");

        response.ShouldBeSuccess();
        (await scenario.State.ProfileTextAsync(user, "favoritesongs")).Should().Be("Style, All Too Well");
    }

    [Theory]
    [InlineData("fav", "favoritesongs", "Style")]
    [InlineData("bae", "bae", "Taylor Swift")]
    public async Task LegacyShow_DisplaysStoredText(string command, string attribute, string value)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.TextAttributeAsync(user, attribute, value);

        var response = await scenario.Discord.SendMessageAsync(user, guild, $"!{command}");

        response.ShouldBeSuccess();
        response.Description.Should().Be(value);
    }

    [Fact]
    public async Task LegacyObsession_DisplaysStoredImage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.TextAttributeAsync(user, "waifu", "https://images.example.invalid/taylor.jpg");

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!waifu");

        response.ShouldBeSuccess();
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("https://images.example.invalid/taylor.jpg");
    }
}
