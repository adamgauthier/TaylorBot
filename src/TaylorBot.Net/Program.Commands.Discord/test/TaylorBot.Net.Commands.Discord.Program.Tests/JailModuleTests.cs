using Discord;
using Discord.Net;
using FakeItEasy;
using FluentAssertions;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
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
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Fact]
        public async Task JailAsync_WhenNoJailRoleSet_ThenReturnsErrorEmbed()
        {
            A.CallTo(() => _jailRepository.GetJailRoleAsync(_guild)).Returns(null);

            var result = (TaylorBotEmbedResult)await _jailModule.JailAsync(A.Fake<IMentionedUserNotAuthorOrClient<IGuildUser>>(o => o.Strict()));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task JailAsync_WhenJailRoleDeleted_ThenReturnsErrorEmbed()
        {
            const ulong AnId = 1;
            A.CallTo(() => _jailRepository.GetJailRoleAsync(_guild)).Returns(new JailRole(new SnowflakeId(AnId)));
            A.CallTo(() => _guild.GetRole(AnId)).Returns(null!);

            var result = (TaylorBotEmbedResult)await _jailModule.JailAsync(A.Fake<IMentionedUserNotAuthorOrClient<IGuildUser>>(o => o.Strict()));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        private IRole SetupValidJailRole()
        {
            const ulong AnId = 1;
            A.CallTo(() => _jailRepository.GetJailRoleAsync(_guild)).Returns(new JailRole(new SnowflakeId(AnId)));
            var jailRole = A.Fake<IRole>();
            A.CallTo(() => jailRole.Id).Returns(AnId);
            A.CallTo(() => _guild.GetRole(AnId)).Returns(jailRole);
            return jailRole;
        }

        private IMentionedUserNotAuthorOrClient<IGuildUser> CreateMentionedUserNotClient(IGuildUser user)
        {
            var mentionedUser = A.Fake<IMentionedUserNotAuthorOrClient<IGuildUser>>(o => o.Strict());
            A.CallTo(() => mentionedUser.GetTrackedUserAsync()).Returns(user);
            return mentionedUser;
        }

        private IMentionedUserNotAuthor<IGuildUser> CreateMentionedUser(IGuildUser user)
        {
            var mentionedUser = A.Fake<IMentionedUserNotAuthor<IGuildUser>>(o => o.Strict());
            A.CallTo(() => mentionedUser.GetTrackedUserAsync()).Returns(user);
            return mentionedUser;
        }

        [Fact]
        public async Task JailAsync_WhenForbiddenJailRoleAdd_ThenReturnsErrorEmbed()
        {
            var jailRole = SetupValidJailRole();
            var user = A.Fake<IGuildUser>(o => o.Strict());
            A.CallTo(() => user.Mention).Returns(string.Empty);
            A.CallTo(() => user.AddRoleAsync(jailRole, null)).ThrowsAsync(
                new HttpException(System.Net.HttpStatusCode.Forbidden, A.Fake<IRequest>(), null, null)
            );

            var result = (TaylorBotEmbedResult)await _jailModule.JailAsync(CreateMentionedUserNotClient(user));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task JailAsync_WhenJailRoleSetAndCanBeAdded_ThenReturnsSuccessEmbed()
        {
            var jailRole = SetupValidJailRole();
            var user = A.Fake<IGuildUser>(o => o.Strict());
            A.CallTo(() => user.Mention).Returns(string.Empty);
            A.CallTo(() => user.AddRoleAsync(jailRole, null)).Returns(Task.CompletedTask);

            var result = (TaylorBotEmbedResult)await _jailModule.JailAsync(CreateMentionedUserNotClient(user));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task Free_WhenJailRoleSetAndCanBeRemoved_ThenReturnsSuccessEmbed()
        {
            var jailRole = SetupValidJailRole();
            var user = A.Fake<IGuildUser>(o => o.Strict());
            A.CallTo(() => user.Mention).Returns(string.Empty);
            A.CallTo(() => user.RoleIds).Returns(new[] { jailRole.Id });
            A.CallTo(() => user.RemoveRoleAsync(jailRole, null)).Returns(Task.CompletedTask);

            var result = (TaylorBotEmbedResult)await _jailModule.FreeAsync(CreateMentionedUser(user));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task SetAsync_ThenReturnsSuccessEmbed()
        {
            const ulong AnId = 1;
            var role = A.Fake<IRole>();
            A.CallTo(() => role.Id).Returns(AnId);
            A.CallTo(() => _jailRepository.SetJailRoleAsync(_guild, role)).Returns(default);

            var result = (TaylorBotEmbedResult)await _jailModule.SetAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }
    }
}
