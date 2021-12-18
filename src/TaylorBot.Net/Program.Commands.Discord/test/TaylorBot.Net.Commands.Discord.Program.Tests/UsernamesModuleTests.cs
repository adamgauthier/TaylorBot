using Discord;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class UsernamesModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IMessageChannel _channel = A.Fake<IMessageChannel>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
        private readonly IUsernameHistoryRepository _usernameHistoryRepository = A.Fake<IUsernameHistoryRepository>(o => o.Strict());
        private readonly UsernamesModule _usernamesModule;

        public UsernamesModuleTests()
        {
            _usernamesModule = new UsernamesModule(new SimpleCommandRunner(), _usernameHistoryRepository);
            _usernamesModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.Channel).Returns(_channel);
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Fact]
        public async Task GetAsync_WhenUsernameHistoryPrivate_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(_commandUser)).Returns(true);

            var result = (await _usernamesModule.GetAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task GetAsync_WhenUsernameHistoryPublic_ThenReturnsPageEmbedWithUsername()
        {
            const string AUsername = "Enchanted13";

            A.CallTo(() => _usernameHistoryRepository.IsUsernameHistoryHiddenFor(_commandUser)).Returns(false);
            A.CallTo(() => _usernameHistoryRepository.GetUsernameHistoryFor(_commandUser, 75)).Returns(new[] {
                new IUsernameHistoryRepository.UsernameChange(Username: AUsername, ChangedAt: DateTimeOffset.Now.AddDays(-1))
            });

            var result = (await _usernamesModule.GetAsync()).GetResult<PageMessageResult>();
            await result.PageMessage.SendAsync(_commandUser, _channel);

            A.CallTo(() => _channel.SendMessageAsync(
                null,
                false,
                A<Embed>.That.Matches(e => e.Description.Contains(AUsername)),
                null,
                null,
                null,
                null,
                null,
                null
            )).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PrivateAsync_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _usernameHistoryRepository.HideUsernameHistoryFor(_commandUser)).Returns(default);

            var result = (await _usernamesModule.PrivateAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task PublicAsync_ThenReturnsSuccessEmbed()
        {
            A.CallTo(() => _usernameHistoryRepository.UnhideUsernameHistoryFor(_commandUser)).Returns(default);

            var result = (await _usernamesModule.PublicAsync()).GetResult<EmbedResult>();

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
