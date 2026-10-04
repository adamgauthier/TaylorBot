using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.Search;

[Trait("Command", "imgur")]
[Trait("Command", "urbandictionary")]
[Trait("Command", "wolframalpha")]
[Trait("Command", "youtube")]
public sealed class SearchCommandTests(DataServices data)
{
    [Fact]
    public async Task Imgur_UploadsSuppliedLinkAndDisplaysReturnedImage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.ImgurUpload();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "imgur",
            arguments: [SlashArgument.Text("link", "https://example.invalid/photo.png")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain("https://i.imgur.com/synthetic.png");
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("https://i.imgur.com/synthetic.png");
        scenario.External.Requests.Should().ContainSingle().Which.Body.Should().Be("image=https%3A%2F%2Fexample.invalid%2Fphoto.png");
    }

    [Fact]
    public async Task Imgur_MissingImageExplainsRequiredInput()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "imgur");

        response.ShouldBeError();
        response.Description.Should().Contain("exactly one").And.Contain("file").And.Contain("link");
        scenario.External.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Imgur_ResolvesAttachedFileAndUploadsItsUrl()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(username: "Recipient");
        scenario.External.ImgurUpload();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "imgur",
            arguments: [SlashArgument.Attachment("file", "https://example.invalid/photo.png")], dm: new(recipient, user));

        response.ShouldBeSuccess();
        response.Description.Should().Contain("https://i.imgur.com/synthetic.png");
        scenario.External.Requests.Should().ContainSingle().Which.Body.Should().Be("image=https%3A%2F%2Fexample.invalid%2Fphoto.png");
    }

    [Fact]
    public async Task Imgur_RejectsConflictingImageSourcesWithoutUploading()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "imgur", arguments:
        [
            SlashArgument.Attachment("file", "https://example.invalid/photo.png"),
            SlashArgument.Text("link", "https://example.invalid/other.png"),
        ]);

        response.ShouldBeError();
        response.Description.Should().Contain("exactly one");
        scenario.External.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData("we don't support that file type!", "file type")]
    [InlineData("file is over the size limit", "too large")]
    [InlineData("unavailable", "unexpected error")]
    public async Task Imgur_ServiceRejectionExplainsTheFailure(string error, string expected)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.ImgurError(error);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "imgur",
            arguments: [SlashArgument.Text("link", "https://example.invalid/photo.png")]);

        response.ShouldBeError();
        response.Description.Should().Contain(expected);
    }

    [Fact]
    public async Task UrbanDictionary_NavigatesActualDefinitionsAndCancels()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.UrbanDefinitions();

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "urbandictionary",
            arguments: [SlashArgument.Text("search", "swiftie")]);
        var next = await scenario.Discord.ClickAsync(user, first, "Next");
        var previous = await scenario.Discord.ClickAsync(user, next, "Previous");
        var cancelled = await scenario.Discord.ClickAsync(user, previous, "Cancel");

        first.Description.Should().Be("A fan of Taylor Swift.");
        first.Field("Votes").Should().Contain("13").And.Contain("2");
        next.Description.Should().Be("Someone who knows every bridge.");
        previous.Description.Should().Be(first.Description);
        cancelled.ShouldBeDeleted();
        scenario.External.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task UrbanDictionary_EmptyResultsExplainMissingDefinition()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.UrbanEmpty();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "urbandictionary",
            arguments: [SlashArgument.Text("search", "swiftie")]);

        response.ShouldBeError();
        response.Description.Should().Contain("No definition found");
    }

    [Fact]
    public async Task WolframAlpha_DisplaysParsedQuestionAndAnswerImage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.WolframAnswer();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "wolframalpha",
            arguments: [SlashArgument.Text("question", "two plus two")]);

        response.Embed.GetProperty("title").GetString().Should().Be("2 + 2");
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("https://example.invalid/result.png");
    }

    [Fact]
    public async Task WolframAlpha_UnrecognizedQuestionExplainsFailure()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.WolframAnswer(understood: false);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "wolframalpha",
            arguments: [SlashArgument.Text("question", "two plus two")]);

        response.ShouldBeError();
        response.Description.Should().Contain("did not understand").And.Contain("two plus two");
    }

    [Fact]
    public async Task YouTube_NavigatesVideosAndCancels()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.YouTubeResults("synthetic01", "synthetic02");

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "youtube", arguments: [SlashArgument.Text("search", "Taylor")]);
        var next = await scenario.Discord.ClickAsync(user, first, "Next");
        var cancelled = await scenario.Discord.ClickAsync(user, next, "Cancel");

        first.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic01");
        next.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic02");
        cancelled.ShouldBeDeleted();
        scenario.External.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task YouTube_EmptyResultsExplainMissingVideos()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        scenario.External.YouTubeResults();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "youtube", arguments: [SlashArgument.Text("search", "Taylor")]);

        response.ShouldBeError();
        response.Description.Should().Contain("No YouTube video found");
    }

    [Fact]
    public async Task UserInstalledYouTube_NavigatesAndCancelsInAnotherUsersDm()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var recipient = await scenario.Given.UserAsync(username: "Recipient");
        scenario.External.YouTubeResults("synthetic01", "synthetic02");

        var first = await scenario.Discord.InvokeSlashCommandAsync(user, "youtube",
            arguments: [SlashArgument.Text("search", "Taylor")], dm: new(recipient, user));
        var next = await scenario.Discord.ClickAsync(user, first, "Next");
        var cancelled = await scenario.Discord.ClickAsync(user, next, "Cancel");

        first.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic01");
        next.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic02");
        cancelled.ShouldBeDeleted();
        scenario.External.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task LegacyYouTube_NavigatesReactionsAndDeletesCancelledMessage()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        scenario.External.YouTubeResults("synthetic01", "synthetic02");
        scenario.DiscordApi.ExpectLegacyPageControls(guild);
        scenario.DiscordApi.ExpectLegacyPageEdit(guild);
        scenario.DiscordApi.ExpectLegacyPageEdit(guild);
        scenario.DiscordApi.ExpectRequest("DELETE", $"channels/{guild.ChannelId}/messages/{DiscordApi.ResponseMessageId}");

        var first = await scenario.Discord.SendMessageAsync(user, guild, "!youtube Taylor");
        var next = await scenario.Discord.ReactAsync(user, first, "Next");
        var previous = await scenario.Discord.ReactAsync(user, next, "Previous");
        var cancelled = await scenario.Discord.ReactAsync(user, previous, "Cancel");

        first.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic01").And.Contain("higher daily limit");
        next.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic02");
        previous.Message.GetProperty("content").GetString().Should().Contain("https://youtu.be/synthetic01");
        cancelled.Requests.Should().ContainSingle().Which.Method.Should().Be("DELETE");
        scenario.External.Requests.Should().ContainSingle();
    }
}
