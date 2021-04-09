﻿using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.LastFm.TypeReaders;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.PostExecution;
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
        public async Task NowPlayingAsync_WhenLastFmLogInRequiredError_ThenReturnsErrorEmbed()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new LastFmLogInRequiredErrorResult());

            var result = (TaylorBotEmbedResult)await _lastFmModule.NowPlayingAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task NowPlayingAsync_WhenLastFmError_ThenReturnsErrorEmbed()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetMostRecentScrobbleAsync(lastFmUsername.Username)).Returns(new LastFmGenericErrorResult("Unknown"));

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
                TotalScrobbles: 100,
                new MostRecentScrobble(
                    TrackName: "All Too Well",
                    TrackUrl: "https://www.last.fm/music/Taylor+Swift/_/All+Too+Well",
                    TrackImageUrl: "https://lastfm.freetls.fastly.net/i/u/174s/e12f82141c2227dd6dce2f7c7a18c101.png",
                    Artist: new ScrobbleArtist("Taylor Swift", "https://www.last.fm/music/Taylor+Swift"),
                    IsNowPlaying: true
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
            var artist = new TopArtist(Name: "Taylor Swift", ArtistUrl: new Uri("https://www.last.fm/music/Taylor+Swift"), PlayCount: 15);
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
        public async Task ArtistsAsync_WhenLongArtistNames_ThenReturnsSuccessEmbedWithArtistInformation()
        {
            var period = LastFmPeriod.SixMonth;
            var lastFmUsername = new LastFmUsername("taylorswift");
            var artist = new TopArtist(
                Name: "Anthony Ramos, Lin-Manuel Miranda, Jon Rua, Leslie Odom, Jr. & Original Broadway Cast of \"Hamilton\"",
                ArtistUrl: new Uri("https://www.last.fm/music/Anthony+Ramos,+Lin-Manuel+Miranda,+Jon+Rua,+Leslie+Odom,+Jr.+&+Original+Broadway+Cast+of+%22Hamilton%22"),
                PlayCount: 15
            );
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetTopArtistsAsync(lastFmUsername.Username, period)).Returns(new TopArtistsResult(Enumerable.Repeat(artist, 10).ToList()));

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
                Name: "All Too Well",
                TrackUrl: new Uri("https://www.last.fm/music/Taylor+Swift/_/All+Too+Well"),
                PlayCount: 22,
                ArtistName: "Taylor Swift",
                ArtistUrl: new Uri("https://www.last.fm/music/Taylor+Swift")
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
        public async Task TracksAsync_WhenLongArtistNames_ThenReturnsSuccessEmbedWithTrackInformation()
        {
            var period = LastFmPeriod.OneMonth;
            var lastFmUsername = new LastFmUsername("taylorswift");
            var track = new TopTrack(
                Name: "Ten Duel Commandments",
                TrackUrl: new Uri("https://www.last.fm/music/Anthony+Ramos,+Lin-Manuel+Miranda,+Jon+Rua,+Leslie+Odom,+Jr.+&+Original+Broadway+Cast+of+%22Hamilton%22/_/Ten+Duel+Commandments"),
                PlayCount: 22,
                ArtistName: "Anthony Ramos, Lin-Manuel Miranda, Jon Rua, Leslie Odom, Jr. & Original Broadway Cast of \"Hamilton\"",
                ArtistUrl: new Uri("https://www.last.fm/music/Anthony+Ramos,+Lin-Manuel+Miranda,+Jon+Rua,+Leslie+Odom,+Jr.+&+Original+Broadway+Cast+of+%22Hamilton%22")
            );
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetTopTracksAsync(lastFmUsername.Username, period)).Returns(new TopTracksResult(Enumerable.Repeat(track, 10).ToList()));

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
        public async Task AlbumsAsync_ThenReturnsSuccessEmbedWithAlbumInformation()
        {
            var period = LastFmPeriod.OneMonth;
            var lastFmUsername = new LastFmUsername("taylorswift");
            var albumImageUrl = "https://lastfm.freetls.fastly.net/i/u/174s/8a89bf00eff0d1912f33164d3a15071f.png";
            var album = new TopAlbum(
                Name: "Lover",
                AlbumUrl: new Uri("https://www.last.fm/music/Taylor+Swift/Lover"),
                AlbumImageUrl: new Uri(albumImageUrl),
                PlayCount: 13,
                ArtistName: "Taylor Swift",
                ArtistUrl: new Uri("https://www.last.fm/music/Taylor+Swift")
            );
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period)).Returns(new TopAlbumsResult(new[] { album }));

            var result = (TaylorBotEmbedResult)await _lastFmModule.AlbumsAsync(period);

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
        public async Task AlbumsAsync_WhenLongAlbumNames_ThenReturnsSuccessEmbedWithAlbumInformation()
        {
            var period = LastFmPeriod.OneMonth;
            var lastFmUsername = new LastFmUsername("taylorswift");
            var albumImageUrl = "https://lastfm.freetls.fastly.net/i/u/770x0/432041b069019cbebbf6328cd2703c3d.webp#432041b069019cbebbf6328cd2703c3d";
            var album = new TopAlbum(
                Name: "Hamilton: An American Musical (Original Broadway Cast Recording)",
                AlbumUrl: new Uri("https://www.last.fm/music/Original+Broadway+Cast+of+%22Hamilton%22/Hamilton:+An+American+Musical+(Original+Broadway+Cast+Recording%29"),
                AlbumImageUrl: new Uri(albumImageUrl),
                PlayCount: 13,
                ArtistName: "Original Broadway Cast of \"Hamilton\"",
                ArtistUrl: new Uri("https://www.last.fm/music/Original+Broadway+Cast+of+%22Hamilton%22")
            );
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);
            A.CallTo(() => _lastFmClient.GetTopAlbumsAsync(lastFmUsername.Username, period)).Returns(new TopAlbumsResult(Enumerable.Repeat(album, 10).ToList()));

            var result = (TaylorBotEmbedResult)await _lastFmModule.AlbumsAsync(period);

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
