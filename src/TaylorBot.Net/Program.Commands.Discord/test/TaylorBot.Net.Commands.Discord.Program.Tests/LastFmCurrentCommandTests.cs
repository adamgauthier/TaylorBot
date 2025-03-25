using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class LastFmCurrentCommandTests
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;
    private readonly IOptionsMonitor<LastFmOptions> _options = A.Fake<IOptionsMonitor<LastFmOptions>>(o => o.Strict());
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly ILastFmClient _lastFmClient = A.Fake<ILastFmClient>(o => o.Strict());
    private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
    private readonly LastFmCurrentCommand _lastFmCurrentCommand;

    public LastFmCurrentCommandTests()
    {
        _lastFmCurrentCommand = new(_options, new(_lastFmPeriodStringMapper), _lastFmUsernameRepository, _lastFmClient);
    }

    [Fact]
    public async Task Current_WhenUsernameNotSet_ThenReturnsErrorEmbed()
    {
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(null);

        var result = (EmbedResult)await _lastFmCurrentCommand.Current(_commandUser).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task Current_WhenLastFmLogInRequiredError_ThenReturnsErrorEmbed()
    {
        LastFmUsername lastFmUsername = new("taylorswift");
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new LastFmLogInRequiredErrorResult());

        var result = (EmbedResult)await _lastFmCurrentCommand.Current(_commandUser).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task Current_WhenLastFmError_ThenReturnsErrorEmbed()
    {
        LastFmUsername lastFmUsername = new("taylorswift");
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new LastFmGenericErrorResult("Unknown"));

        var result = (EmbedResult)await _lastFmCurrentCommand.Current(_commandUser).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task Current_WhenNoScrobbles_ThenReturnsErrorEmbed()
    {
        LastFmUsername lastFmUsername = new("taylorswift");
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new MostRecentScrobbleResult(0, null));

        var result = (EmbedResult)await _lastFmCurrentCommand.Current(_commandUser).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task Current_ThenReturnsSuccessEmbed()
    {
        LastFmUsername lastFmUsername = new("taylorswift");
        A.CallTo(() => _options.CurrentValue).Returns(new LastFmOptions { LastFmEmbedFooterIconUrl = "https://last.fm./icon.png" });
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new MostRecentScrobbleResult(
            TotalScrobbles: 100,
            new MostRecentScrobble(
                TrackName: "All Too Well",
                TrackUrl: "https://www.last.fm/music/Taylor+Swift/_/All+Too+Well",
                TrackImageUrl: "https://lastfm.freetls.fastly.net/i/u/174s/e12f82141c2227dd6dce2f7c7a18c101.png",
                Artist: new ScrobbleArtist("Taylor Swift", "https://www.last.fm/music/Taylor+Swift"),
                IsNowPlaying: true
            )
        ));

        var result = (EmbedResult)await _lastFmCurrentCommand.Current(_commandUser).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
