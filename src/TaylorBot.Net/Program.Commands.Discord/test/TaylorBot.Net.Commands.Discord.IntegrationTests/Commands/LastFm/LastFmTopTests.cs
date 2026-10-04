using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Discord;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.LastFm;

[Trait("Command", "lastfm albums")]
[Trait("Command", "lastfm artists")]
[Trait("Command", "lastfm tracks")]
public sealed class LastFmTopTests(DataServices data)
{
    [Fact]
    public async Task EmptyAlbums_ExplainsMissingListeningHistoryForPeriod()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.NoTopAlbums();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm albums");

        response.ShouldBeError();
        response.Description.Should().Contain("doesn't have scrobbles");
    }

    [Theory]
    [InlineData("Lover", "Taylor Swift", 1)]
    [InlineData("An Extremely Long Album Title (Original Cast Recording)", "An Extremely Long Artist Name and the Orchestra", 10)]
    public async Task Albums_PreservesAlbumAndArtistDetailsWithinEmbedLimit(string album, string artist, int count)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.TopAlbums(album, artist, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm albums",
            arguments: [SlashArgument.Text("period", "1month")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(album).And.Contain(artist).And.Contain("13")
            .And.Contain(LastFmResponses.AlbumUrl(album, artist)).And.Contain(LastFmResponses.MusicUrl(artist));
        response.Description.Length.Should().BeLessThanOrEqualTo(4096);
        response.Embed.GetProperty("thumbnail").GetProperty("url").GetString().Should().Be(LastFmResponses.ImageUrl);
    }

    [Theory]
    [InlineData("Taylor Swift", 1)]
    [InlineData("An Extremely Long Artist Name Featuring Another Performer and the Original Broadway Orchestra", 10)]
    public async Task Artists_PreservesNameUrlAndPlayCountWithinEmbedLimit(string artist, int count)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.TopArtists(artist, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm artists",
            arguments: [SlashArgument.Text("period", "6month")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(artist).And.Contain("15").And.Contain(LastFmResponses.MusicUrl(artist));
        response.Description.Length.Should().BeLessThanOrEqualTo(4096);
    }

    [Theory]
    [InlineData("All Too Well", "Taylor Swift", 1)]
    [InlineData("An Extremely Long Track Title", "An Extremely Long Artist Name Featuring Another Performer and the Original Broadway Orchestra", 10)]
    public async Task Tracks_PreservesTrackAndArtistDetailsWithinEmbedLimit(string track, string artist, int count)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.TopTracks(track, artist, count);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm tracks",
            arguments: [SlashArgument.Text("period", "1month")]);

        response.ShouldBeSuccess();
        response.Description.Should().Contain(track).And.Contain(artist).And.Contain("22")
            .And.Contain(LastFmResponses.SongUrl(track, artist)).And.Contain(LastFmResponses.MusicUrl(artist));
        response.Description.Length.Should().BeLessThanOrEqualTo(4096);
    }

    [Fact]
    public async Task LegacyAlbums_ParsesPeriodAndDisplaysListeningHistory()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LastFmAsync(user);
        scenario.External.TopAlbums("Lover", "Taylor Swift", count: 1);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!lastfm albums 1month");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Lover").And.Contain("Taylor Swift").And.Contain("13");
    }

    [Fact]
    public async Task LegacyArtists_ParsesPeriodAndDisplaysListeningHistory()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LastFmAsync(user);
        scenario.External.TopArtists("Taylor Swift", count: 1);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!lastfm artists 6month");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("Taylor Swift").And.Contain("15");
    }

    [Fact]
    public async Task LegacyTracks_ParsesPeriodAndDisplaysListeningHistory()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        var guild = await scenario.Given.GuildAsync(user);
        await scenario.Given.LastFmAsync(user);
        scenario.External.TopTracks("All Too Well", "Taylor Swift", count: 1);

        var response = await scenario.Discord.SendMessageAsync(user, guild, "!lastfm tracks 1month");

        response.ShouldBeSuccess();
        response.Description.Should().Contain("All Too Well").And.Contain("Taylor Swift").And.Contain("22");
    }
}
