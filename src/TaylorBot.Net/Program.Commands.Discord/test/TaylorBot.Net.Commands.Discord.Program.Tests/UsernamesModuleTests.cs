using Discord;
using FakeItEasy;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Discord.Program.UsernameHistory.Domain;
using TaylorBot.Net.Commands.PostExecution;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class UsernamesModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IMessageChannel _channel = A.Fake<IMessageChannel>();
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IUsernameHistoryRepository _usernameHistoryRepository = A.Fake<IUsernameHistoryRepository>(o => o.Strict());
        private readonly UsernamesModule _usernamesModule;

        public UsernamesModuleTests()
        {
            _usernamesModule = new UsernamesModule(_usernameHistoryRepository);
            _usernamesModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.Channel).Returns(_channel);
        }

        [Fact]
        public async Task GetAsync_ThenReturnsEmbedWithUsername()
        {
            const string AUsername = "Enchanted13";

            A.CallTo(() => _usernameHistoryRepository.GetUsernameHistoryFor(_commandUser, 75)).Returns(new[] {
                new IUsernameHistoryRepository.UsernameChange(Username: AUsername, ChangedAt: DateTimeOffset.Now.AddDays(-1))
            });

            var result = (TaylorBotPageMessageResult)await _usernamesModule.GetAsync();
            await result.PageMessage.SendAsync(_commandUser, _channel);

            A.CallTo(() => _channel.SendMessageAsync(
                null,
                false,
                A<Embed>.That.Matches(e => e.Description.Contains(AUsername)),
                null,
                null
            )).MustHaveHappenedOnceExactly();
        }
    }
}
