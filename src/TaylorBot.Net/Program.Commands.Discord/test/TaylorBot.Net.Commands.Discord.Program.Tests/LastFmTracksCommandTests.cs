using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class LastFmTracksCommandTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
        private readonly ILastFmClient _lastFmClient = A.Fake<ILastFmClient>(o => o.Strict());
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
        private readonly LastFmTracksCommand _lastFmTracksCommand;

        public LastFmTracksCommandTests()
        {
            _lastFmTracksCommand = new(new(_lastFmPeriodStringMapper), _lastFmUsernameRepository, _lastFmClient, _lastFmPeriodStringMapper);
        }

        [Fact]
        public async Task Tracks_ThenReturnsSuccessEmbedWithTrackInformation()
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

            var result = (EmbedResult)await _lastFmTracksCommand.Tracks(period, _commandUser, isLegacyCommand: false).RunAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            result.Embed.Description
                .Should().Contain(track.Name)
                .And.Contain(track.PlayCount.ToString())
                .And.Contain(track.TrackUrl.ToString())
                .And.Contain(track.ArtistName)
                .And.Contain(track.ArtistUrl.ToString());
        }

        [Fact]
        public async Task Tracks_WhenLongArtistNames_ThenReturnsSuccessEmbedWithTrackInformation()
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

            var result = (EmbedResult)await _lastFmTracksCommand.Tracks(period, _commandUser, isLegacyCommand: false).RunAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
            result.Embed.Description
                .Should().Contain(track.Name)
                .And.Contain(track.PlayCount.ToString())
                .And.Contain(track.TrackUrl.ToString())
                .And.Contain(track.ArtistName)
                .And.Contain(track.ArtistUrl.ToString());
        }
    }
}
