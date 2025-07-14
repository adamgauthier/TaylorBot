using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.User;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class LastFmAlbumsCommandTests
{
    private readonly DiscordUser _commandUser = CommandUtils.AUser;
    private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
    private readonly ILastFmClient _lastFmClient = A.Fake<ILastFmClient>(o => o.Strict());
    private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
    private readonly LastFmAlbumsSlashCommand _lastFmAlbumsCommand;
    private readonly RunContext _context;

    public LastFmAlbumsCommandTests()
    {
        _lastFmAlbumsCommand = new(new(_lastFmPeriodStringMapper, CommandUtils.Mentioner), _lastFmUsernameRepository, _lastFmClient, _lastFmPeriodStringMapper);
        _context = CommandUtils.CreateTestContext(_lastFmAlbumsCommand);
    }

    [Fact]
    public async Task Albums_ThenReturnsSuccessEmbedWithAlbumInformation()
    {
        var period = LastFmPeriod.OneMonth;
        LastFmUsername lastFmUsername = new("taylorswift");
        var albumImageUrl = "https://lastfm.freetls.fastly.net/i/u/174s/8a89bf00eff0d1912f33164d3a15071f.png";
        TopAlbum album = new(
            Name: "Lover",
            AlbumUrl: new Uri("https://www.last.fm/music/Taylor+Swift/Lover"),
            AlbumImageUrl: new Uri(albumImageUrl),
            PlayCount: 13,
            ArtistName: "Taylor Swift",
            ArtistUrl: new Uri("https://www.last.fm/music/Taylor+Swift")
        );
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period)).Returns(new TopAlbumsResult([album]));

        var result = (EmbedResult)await _lastFmAlbumsCommand.Albums(period, _commandUser, _context).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Embed.Thumbnail!.Value.Url.Should().Be(albumImageUrl);
        result.Embed.Description
            .Should().Contain(album.Name)
            .And.Contain(album.AlbumUrl.ToString())
            .And.Contain(album.PlayCount.ToString())
            .And.Contain(album.ArtistName)
            .And.Contain(album.ArtistUrl.ToString());
    }

    [Fact]
    public async Task Albums_WhenLongAlbumNames_ThenReturnsSuccessEmbedWithAlbumInformation()
    {
        var period = LastFmPeriod.OneMonth;
        LastFmUsername lastFmUsername = new("taylorswift");
        var albumImageUrl = "https://lastfm.freetls.fastly.net/i/u/770x0/432041b069019cbebbf6328cd2703c3d.webp#432041b069019cbebbf6328cd2703c3d";
        TopAlbum album = new(
            Name: "Hamilton: An American Musical (Original Broadway Cast Recording)",
            AlbumUrl: new Uri("https://www.last.fm/music/Original+Broadway+Cast+of+%22Hamilton%22/Hamilton:+An+American+Musical+(Original+Broadway+Cast+Recording%29"),
            AlbumImageUrl: new Uri(albumImageUrl),
            PlayCount: 13,
            ArtistName: "Original Broadway Cast of \"Hamilton\"",
            ArtistUrl: new Uri("https://www.last.fm/music/Original+Broadway+Cast+of+%22Hamilton%22")
        );
        A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
        A.CallTo(() => _lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period)).Returns(new TopAlbumsResult([.. Enumerable.Repeat(album, 10)]));

        var result = (EmbedResult)await _lastFmAlbumsCommand.Albums(period, _commandUser, _context).RunAsync();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        result.Embed.Thumbnail!.Value.Url.Should().Be(albumImageUrl);
        result.Embed.Description
            .Should().Contain(album.Name)
            .And.Contain(album.AlbumUrl.ToString())
            .And.Contain(album.PlayCount.ToString())
            .And.Contain(album.ArtistName)
            .And.Contain(album.ArtistUrl.ToString());
    }
}
