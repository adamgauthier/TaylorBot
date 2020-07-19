using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class FrameworkModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IGuild _commandGuild = A.Fake<IGuild>(o => o.Strict());
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly ICommandPrefixRepository _commandPrefixRepository = A.Fake<ICommandPrefixRepository>(o => o.Strict());
        private readonly FrameworkModule _frameworkModule;

        public FrameworkModuleTests()
        {
            _frameworkModule = new FrameworkModule(_commandPrefixRepository);
            _frameworkModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.Guild).Returns(_commandGuild);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
        }

        [Fact]
        public async Task PrefixAsync_ThenReturnsEmbedWithNewPrefix()
        {
            var newPrefix = ".";
            A.CallTo(() => _commandPrefixRepository.ChangeGuildPrefixAsync(_commandGuild, newPrefix)).Returns(default);

            var result = (TaylorBotEmbedResult)await _frameworkModule.PrefixAsync(new Word(newPrefix));

            result.Embed.Description.Should().Contain(newPrefix);
        }
    }
}
