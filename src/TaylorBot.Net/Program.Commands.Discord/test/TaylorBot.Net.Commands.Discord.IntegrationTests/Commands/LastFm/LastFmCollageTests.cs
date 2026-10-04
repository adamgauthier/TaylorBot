using System.Net;
using System.Text.Json;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.LastFm;

[Trait("Command", "lastfm collage")]
public sealed class LastFmCollageTests(DataServices data)
{
    [Fact]
    public async Task UnsetUsername_DoesNotContactCollageService()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm collage");

        response.ShouldBeError();
        response.Description.Should().Contain("username is not set");
        scenario.External.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Collage_UploadsDownloadedImageAndReferencesAttachment()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        var image = scenario.External.Collage();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm collage");

        response.ShouldBeSuccess();
        response.Embed.GetProperty("image").GetProperty("url").GetString().Should().Be("attachment://collage.png");
        var upload = response.Requests.SelectMany(request => request.Attachments ?? []).Should().ContainSingle().Which;
        upload.Name.Should().Be("collage.png");
        upload.Bytes.Should().Equal(image);
        var request = JsonSerializer.Deserialize<Dictionary<string, string>>(scenario.External.Requests.Single(request => request.Method == "POST").Body!)!;
        request.Should().Contain("username", "taylorswift").And.Contain("period", "1week").And.Contain("rowNum", "3").And.Contain("colNum", "3");
        request.Should().Contain("type", "albums").And.Contain("showName", "true").And.Contain("hideMissing", "false");
    }

    [Fact]
    public async Task NoScrobbles_SuggestsLongerPeriodWithoutDownloading()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.CollageWithoutScrobbles();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm collage");

        response.ShouldBeError();
        response.Description.Should().Contain("No scrobbles").And.Contain("longer period").And.Contain("last year").And.Contain("all time");
        response.Description.Should().NotContain("service is currently unavailable");
        scenario.External.Requests.Should().ContainSingle().Which.Method.Should().Be("POST");
        response.Requests.SelectMany(request => request.Attachments ?? []).Should().BeEmpty();
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, """{"message":"Unknown resource"}""")]
    [InlineData(HttpStatusCode.ServiceUnavailable, """{"message":"The account does not have any scrobbles for the time period specified."}""")]
    [InlineData(HttpStatusCode.NotFound, "<html>Service unavailable</html>")]
    [InlineData(HttpStatusCode.NotFound, "{}")]
    [InlineData(HttpStatusCode.NotFound, "null")]
    [InlineData(HttpStatusCode.NotFound, "[]")]
    [InlineData(HttpStatusCode.NotFound, """{"message":13}""")]
    public async Task UnrecognizedFailure_KeepsGenericMessageWithoutDownloading(HttpStatusCode status, string body)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.CollageError(status, body);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm collage");

        response.ShouldBeError();
        response.Description.Should().Contain("collage service is currently unavailable").And.NotContain("longer period");
        scenario.External.Requests.Should().ContainSingle().Which.Method.Should().Be("POST");
        response.Requests.SelectMany(request => request.Attachments ?? []).Should().BeEmpty();
    }

    [Theory]
    [InlineData("12month", 5, "1year")]
    [InlineData("all", 4, "forever")]
    public async Task SelectedPeriodAndSize_AreSentToCollageService(string period, int size, string expectedPeriod)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        var image = scenario.External.Collage();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm collage",
            arguments: [SlashArgument.Text("period", period), SlashArgument.Integer("size", size)]);

        response.ShouldBeSuccess();
        response.Requests.SelectMany(request => request.Attachments ?? []).Should().ContainSingle().Which.Bytes.Should().Equal(image);
        var request = JsonSerializer.Deserialize<Dictionary<string, string>>(scenario.External.Requests.Single(request => request.Method == "POST").Body!)!;
        request.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["username"] = "taylorswift",
            ["period"] = expectedPeriod,
            ["rowNum"] = $"{size}",
            ["colNum"] = $"{size}",
            ["type"] = "albums",
            ["showName"] = "true",
            ["hideMissing"] = "false",
        });
    }
}
