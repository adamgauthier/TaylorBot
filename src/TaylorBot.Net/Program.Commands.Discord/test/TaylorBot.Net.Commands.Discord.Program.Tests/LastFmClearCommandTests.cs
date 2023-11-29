using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.LastFm.Domain;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class LastFmClearCommandTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly ILastFmUsernameRepository _lastFmUsernameRepository = A.Fake<ILastFmUsernameRepository>(o => o.Strict());
        private readonly LastFmClearCommand _lastFmClearCommand;

        public LastFmClearCommandTests()
        {
            _lastFmClearCommand = new(_lastFmUsernameRepository);
        }

        [Fact]
        public async Task Clear_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _lastFmUsernameRepository.ClearLastFmUsernameAsync(_commandUser)).Returns(default);

            var result = (EmbedResult)await _lastFmClearCommand.Clear(_commandUser, isLegacyCommand: false).RunAsync();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
