using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class LastFmArtistsCommandTests
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly ILastFmClient _lastFmClient = A.Fake<ILastFmClient>(o => o.Strict());
    private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
    private readonly LastFmArtistsCommand _lastFmArtistsCommand;

    public LastFmArtistsCommandTests()
    {
        _lastFmArtistsCommand = new(new(_lastFmPeriodStringMapper), _lastFmUsernameRepository, _lastFmClient, _lastFmPeriodStringMapper);
    }

    [Fact]
    public async Task Artists_ThenReturnsSuccessEmbedWithArtistInformation()
    {
        var period = LastFmPeriod.SixMonth;
        LastFmUsername lastFmUsername = new("taylorswift");
        var artist = new TopArtist(Name: "Taylor Swift", ArtistUrl: new Uri("https://www.last.fm/music/Taylor+Swift"), PlayCount: 15);
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period)).Returns(new TopArtistsResult([artist]));

        var result = (EmbedResult)await _lastFmArtistsCommand.Artists(period, _commandUser, isLegacyCommand: false).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Embed.Description
            .Should().Contain(artist.Name)
            .And.Contain(artist.PlayCount.ToString())
            .And.Contain(artist.ArtistUrl.ToString());
    }

    [Fact]
    public async Task Artists_WhenLongArtistNames_ThenReturnsSuccessEmbedWithArtistInformation()
    {
        var period = LastFmPeriod.SixMonth;
        LastFmUsername lastFmUsername = new("taylorswift");
        TopArtist artist = new(
            Name: "Anthony Ramos, Lin-Manuel Miranda, Jon Rua, Leslie Odom, Jr. & Original Broadway Cast of \"Hamilton\"",
            ArtistUrl: new Uri("https://www.last.fm/music/Anthony+Ramos,+Lin-Manuel+Miranda,+Jon+Rua,+Leslie+Odom,+Jr.+&+Original+Broadway+Cast+of+%22Hamilton%22"),
            PlayCount: 15
        );
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period)).Returns(new TopArtistsResult([.. Enumerable.Repeat(artist, 10)]));

        var result = (EmbedResult)await _lastFmArtistsCommand.Artists(period, _commandUser, isLegacyCommand: false).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Embed.Description
            .Should().Contain(artist.Name)
            .And.Contain(artist.PlayCount.ToString())
            .And.Contain(artist.ArtistUrl.ToString());
    }
}
