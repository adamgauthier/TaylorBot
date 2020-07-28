using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.LastFm.TypeReaders;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class LastFmModuleTests
    {
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IOptionsMonitor<LastFmOptions> _options = A.Fake<IOptionsMonitor<LastFmOptions>>(o => o.Strict());
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
        private readonly ILastFmClient _lastFmClient = A.Fake<ILastFmClient>(o => o.Strict());
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new LastFmPeriodStringMapper();
        private readonly LastFmModule _lastFmModule;

        public LastFmModuleTests()
        {
            A.CallTo(() => _commandContext.CommandPrefix).Returns(string.Empty);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            _lastFmModule = new LastFmModule(_options, _lastFmUsernameRepository, _lastFmClient, _lastFmPeriodStringMapper);
            _lastFmModule.SetContext(_commandContext);
        }

        [Fact]
        public async Task NowPlayingAsync_WhenUsernameNotSet_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(null);

            var result = (TaylorBotEmbedResult)await _lastFmModule.NowPlayingAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task NowPlayingAsync_WhenLastFmError_ThenReturnsErrorEmbed()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new LastFmErrorResult("Unknown"));

            var result = (TaylorBotEmbedResult)await _lastFmModule.NowPlayingAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task NowPlayingAsync_WhenNoScrobbles_ThenReturnsErrorEmbed()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new MostRecentScrobbleResult(0, null));

            var result = (TaylorBotEmbedResult)await _lastFmModule.NowPlayingAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task NowPlayingAsync_ThenReturnsSuccessEmbed()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _options.CurrentValue).Returns(new LastFmOptions { LastFmEmbedFooterIconUrl = "https://last.fm./icon.png" });
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new MostRecentScrobbleResult(
                totalScrobbles: 100,
                new MostRecentScrobble(
                    trackName: "All Too Well",
                    trackUrl: "https://www.last.fm/music/Taylor+Swift/_/All+Too+Well",
                    trackImageUrl: "https://lastfm.freetls.fastly.net/i/u/174s/e12f82141c2227dd6dce2f7c7a18c101.png",
                    artist: new ScrobbleArtist("Taylor Swift", "https://www.last.fm/music/Taylor+Swift"),
                    isNowPlaying: true
                )
            ));

            var result = (TaylorBotEmbedResult)await _lastFmModule.NowPlayingAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task SetAsync_ThenReturnsSuccessEmbed()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _lastFmUsernameRepository.SetLastFmUsernameAsync(_commandUser, lastFmUsername)).Returns(default);

            var result = (TaylorBotEmbedResult)await _lastFmModule.SetAsync(lastFmUsername);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task ClearAsync_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _lastFmUsernameRepository.ClearLastFmUsernameAsync(_commandUser)).Returns(default);

            var result = (TaylorBotEmbedResult)await _lastFmModule.ClearAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task ArtistsAsync_ThenReturnsSuccessEmbedWithArtistInformation()
        {
            var period = LastFmPeriod.SixMonth;
            var lastFmUsername = new LastFmUsername("taylorswift");
            var artist = new TopArtist(name: "Taylor Swift", artistUrl: new Uri("https://www.last.fm/music/Taylor+Swift"), playCount: 15);
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period)).Returns(new TopArtistsResult(new[] { artist }));

            var result = (TaylorBotEmbedResult)await _lastFmModule.ArtistsAsync(period);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            result.Embed.Description
                .Should().Contain(artist.Name)
                .And.Contain(artist.PlayCount.ToString())
                .And.Contain(artist.ArtistUrl.ToString());
        }

        [Fact]
        public async Task TracksAsync_ThenReturnsSuccessEmbedWithTrackInformation()
        {
            var period = LastFmPeriod.OneMonth;
            var lastFmUsername = new LastFmUsername("taylorswift");
            var track = new TopTrack(
                name: "All Too Well",
                trackUrl: new Uri("https://www.last.fm/music/Taylor+Swift/_/All+Too+Well"),
                playCount: 22,
                artistName: "Taylor Swift",
                artistUrl: new Uri("https://www.last.fm/music/Taylor+Swift")
            );
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetTopTracksAsync(lastFmUsername.Username, period)).Returns(new TopTracksResult(new[] { track }));

            var result = (TaylorBotEmbedResult)await _lastFmModule.TracksAsync(period);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            result.Embed.Description
                .Should().Contain(track.Name)
                .And.Contain(track.PlayCount.ToString())
                .And.Contain(track.TrackUrl.ToString())
                .And.Contain(track.ArtistName)
                .And.Contain(track.ArtistUrl.ToString());
        }

        [Fact]
        public async Task CollageAsync_WhenUsernameNotSet_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(null);

            var result = (TaylorBotEmbedResult)await _lastFmModule.CollageAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task CollageAsync_ThenReturnsEmbedWithImage()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _options.CurrentValue).Returns(new LastFmOptions { LastFmEmbedFooterIconUrl = "https://last.fm./icon.png" });
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);

            var result = (TaylorBotEmbedResult)await _lastFmModule.CollageAsync();

            result.Embed.Image.Should().NotBeNull();
        }
    }
}
