using Discord;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class JailModuleTests
    {
        private readonly IUser _commandUser = A.Fake<IUser>();
        private readonly IGuild _guild = A.Fake<IGuild>(o => o.Strict());
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IJailRepository _jailRepository = A.Fake<IJailRepository>(o => o.Strict());
        private readonly JailModule _jailModule;

        public JailModuleTests()
        {
            _jailModule = new JailModule(_jailRepository);
            _jailModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.Guild).Returns(_guild);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
        }

        [Fact]
        public async Task SetAsync_ThenReturnsSuccessEmbed()
        {
            const ulong AnId = 1;
            var role = A.Fake<IRole>();
            A.CallTo(() => role.Id).Returns(AnId);
            A.CallTo(() => _jailRepository.SetJailRoleAsync(_guild, role)).Returns(default);

            var result = (TaylorBotEmbedResult)await _jailModule.SetAsync(role);

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
