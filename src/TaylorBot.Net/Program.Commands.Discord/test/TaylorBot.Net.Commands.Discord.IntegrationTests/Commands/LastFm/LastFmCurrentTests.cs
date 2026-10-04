using FluentAssertions;
using TaylorBot.Net.Commands.Discord.IntegrationTests.ExternalApis;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Hosting;
using TaylorBot.Net.Commands.Discord.IntegrationTests.Infrastructure;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Commands.LastFm;

[Trait("Command", "lastfm current")]
public sealed class LastFmCurrentTests(DataServices data)
{
    [Fact]
    public async Task UnsetUsername_ExplainsHowToLinkAccount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm current");

        response.ShouldBeError();
        response.Description.Should().Contain("username is not set");
        scenario.External.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task HiddenRecentTracks_ExplainsPrivacySetting()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.RecentError(error: 17);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm current");

        response.ShouldBeError();
        response.Description.Should().Contain("not public").And.Contain("https://www.last.fm/settings/privacy");
    }

    [Fact]
    public async Task ServiceError_ShowsFailureRatherThanScrobble()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.RecentError(error: 11);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm current");

        response.ShouldBeError();
        response.Description.Should().Contain("Last.fm returned an error");
    }

    [Fact]
    public async Task NoScrobbles_ExplainsEmptyListeningHistory()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.NoRecentScrobbles();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm current");

        response.ShouldBeError();
        response.Description.Should().Contain("doesn't have scrobbles");
    }

    [Fact]
    public async Task RecentScrobble_ShowsArtistTrackAndPlayCount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.RecentScrobble();

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm current");

        response.ShouldBeSuccess();
        response.Field("Artist").Should().Contain(LastFmResponses.ArtistName).And.Contain("https://www.last.fm/music/Taylor%20Swift");
        response.Field("Track").Should().Contain(LastFmResponses.TrackName).And.Contain(LastFmResponses.TrackUrl);
        response.Embed.GetProperty("thumbnail").GetProperty("url").GetString().Should().Be(LastFmResponses.ImageUrl);
        response.Embed.GetProperty("footer").GetProperty("text").GetString().Should().Contain("Now Playing").And.Contain("13");
    }

    [Fact]
    public async Task LegacyCurrent_ResolvesMentionedUsersLinkedAccount()
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var author = await scenario.Given.UserAsync();
        var listener = await scenario.Given.UserAsync(username: "Listener");
        var guild = await scenario.Given.GuildAsync(author);
        await scenario.Given.MemberAsync(guild, listener);
        await scenario.Given.LastFmAsync(listener);
        scenario.External.RecentScrobble();

        var response = await scenario.Discord.SendMessageAsync(author, guild, $"!lastfm <@{listener.Id}>");

        response.ShouldBeSuccess();
        response.Field("Artist").Should().Contain(LastFmResponses.ArtistName);
        response.Field("Track").Should().Contain(LastFmResponses.TrackName);
        response.Embed.GetProperty("author").GetProperty("name").GetString().Should().Be("taylorswift");
    }

    [Theory]
    [InlineData(true, "Now Playing")]
    [InlineData(false, "Last Played")]
    public async Task ExpandedRecentArtist_PreservesUrlAndPlaybackState(bool nowPlaying, string playbackState)
    {
        await using var scenario = await CommandsDiscordScenario.CreateAsync(data, TestContext.Current.CancellationToken);
        var user = await scenario.Given.UserAsync();
        await scenario.Given.LastFmAsync(user);
        scenario.External.RecentScrobble(nowPlaying, expandedArtist: true);

        var response = await scenario.Discord.InvokeSlashCommandAsync(user, "lastfm current");

        response.ShouldBeSuccess();
        response.Field("Artist").Should().Contain(LastFmResponses.ArtistName).And.Contain(LastFmResponses.ArtistUrl);
        response.Field("Track").Should().Contain(LastFmResponses.TrackName);
        response.Embed.GetProperty("thumbnail").GetProperty("url").GetString().Should().Be(LastFmResponses.ImageUrl);
        response.Embed.GetProperty("footer").GetProperty("text").GetString().Should().Contain(playbackState).And.Contain("13");
    }
}
