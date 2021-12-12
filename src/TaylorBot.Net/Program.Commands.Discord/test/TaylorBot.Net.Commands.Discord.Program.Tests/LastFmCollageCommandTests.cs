using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class LastFmCollageCommandTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IOptionsMonitor<LastFmOptions> _options = A.Fake<IOptionsMonitor<LastFmOptions>>(o => o.Strict());
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
        private readonly LastFmPeriodStringMapper _lastFmPeriodStringMapper = new();
        private readonly LastFmCollageCommand _lastFmCollageCommand;

        public LastFmCollageCommandTests()
        {
            _lastFmCollageCommand = new(_lastFmUsernameRepository, new(_lastFmPeriodStringMapper), _lastFmPeriodStringMapper);
        }


        [Fact]
        public async Task CollageAsync_WhenUsernameNotSet_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(null);

            var result = (EmbedResult)await _lastFmCollageCommand.Collage(null, null, _commandUser, isLegacyCommand: false).RunAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task CollageAsync_ThenReturnsEmbedWithImage()
        {
            var lastFmUsername = new LastFmUsername("taylorswift");
            A.CallTo(() => _options.CurrentValue).Returns(new LastFmOptions { LastFmEmbedFooterIconUrl = "https://last.fm./icon.png" });
            A.CallTo(() => _lastFmUsernameRepository.GetLastFmUsernameAsync(_commandUser)).Returns(lastFmUsername);

            var result = (EmbedResult)await _lastFmCollageCommand.Collage(null, null, _commandUser, isLegacyCommand: false).RunAsync();

            result.Embed.Image.Should().NotBeNull();
        }
    }
}
