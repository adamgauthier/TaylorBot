using Discord;
using Discord.Net;
using FakeItEasy;
using FluentAssertions;
using System;
using System.Net;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests
{
    public class AccessibleRolesModuleTests
    {
        private const ulong ARoleId = 13;
        private const string ARoleName = "Regulars";

        private readonly IGuildUser _commandUser = A.Fake<IGuildUser>();
        private readonly IUserMessage _message = A.Fake<IUserMessage>();
        private readonly IGuild _guild = A.Fake<IGuild>(o => o.Strict());
        private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        private readonly IAccessibleRoleRepository _accessibleRoleRepository = A.Fake<IAccessibleRoleRepository>(o => o.Strict());
        private readonly AccessibleRolesModule _accessibleRolesModule;

        public AccessibleRolesModuleTests()
        {
            _accessibleRolesModule = new AccessibleRolesModule(_accessibleRoleRepository);
            _accessibleRolesModule.SetContext(_commandContext);
            A.CallTo(() => _commandContext.Guild).Returns(_guild);
            A.CallTo(() => _commandContext.User).Returns(_commandUser);
            A.CallTo(() => _commandContext.Message).Returns(_message);
            A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        }

        [Fact]
        public async Task GetAsync_WhenUserAlreadyHasRole_ThenReturnsErrorEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(new[] { RoleId });

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task GetAsync_WhenRoleIsNotAccessible_ThenReturnsErrorEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(Array.Empty<ulong>());
            A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(null);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task GetAsync_WhenRoleIsAccessibleWithNoGroup_ThenReturnsSuccessEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(Array.Empty<ulong>());
            A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(new AccessibleRoleWithGroup(group: null));
            A.CallTo(() => _commandUser.AddRoleAsync(role, A<RequestOptions>.Ignored)).Returns(Task.CompletedTask);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task GetAsync_WhenRoleIsAccessibleInGroupThatUserAlreadyHas_ThenReturnsErrorEmbed()
        {
            const ulong RoleId = 1989;
            const ulong AnotherRoleId = 1322;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(new[] { AnotherRoleId });
            A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(new AccessibleRoleWithGroup(
                group: new AccessibleRoleGroup(name: "regions", otherRolesInSameGroup: new[] { new SnowflakeId(AnotherRoleId) })
            ));
            A.CallTo(() => _commandUser.AddRoleAsync(role, A<RequestOptions>.Ignored)).Returns(Task.CompletedTask);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task GetAsync_WhenRoleIsAccessibleButDiscordForbidsAddingRole_ThenReturnsErrorEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(Array.Empty<ulong>());
            A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(new AccessibleRoleWithGroup(group: null));
            A.CallTo(() => _commandUser.AddRoleAsync(role, A<RequestOptions>.Ignored)).Throws(new HttpException(HttpStatusCode.Forbidden, A.Fake<IRequest>()));

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task DropAsync_WhenUserDoesntHaveRole_ThenReturnsErrorEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(Array.Empty<ulong>());

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.DropAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task DropAsync_WhenRoleIsNotAccessible_ThenReturnsErrorEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(new[] { RoleId });
            A.CallTo(() => _accessibleRoleRepository.IsRoleAccessibleAsync(role)).Returns(false);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.DropAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
        }

        [Fact]
        public async Task DropAsync_WhenRoleIsAccessible_ThenReturnsSuccessEmbed()
        {
            const ulong RoleId = 1989;
            var role = CreateFakeRole(RoleId, ARoleName);
            A.CallTo(() => _commandUser.RoleIds).Returns(new[] { RoleId });
            A.CallTo(() => _accessibleRoleRepository.IsRoleAccessibleAsync(role)).Returns(true);
            A.CallTo(() => _commandUser.RemoveRoleAsync(role, A<RequestOptions>.Ignored)).Returns(Task.CompletedTask);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.DropAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task AddAsync_ThenReturnsSuccessEmbed()
        {
            var role = CreateFakeRole(ARoleId, ARoleName);
            A.CallTo(() => _accessibleRoleRepository.AddAccessibleRoleAsync(role)).Returns(default);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.AddAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task RemoveAsync_ThenReturnsSuccessEmbed()
        {
            var role = CreateFakeRole(ARoleId, ARoleName);
            A.CallTo(() => _accessibleRoleRepository.RemoveAccessibleRoleAsync(role)).Returns(default);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.RemoveAsync(new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task GroupAsync_ThenReturnsSuccessEmbed()
        {
            var groupName = new AccessibleGroupName("regions");
            var role = CreateFakeRole(ARoleId, ARoleName);
            A.CallTo(() => _accessibleRoleRepository.AddOrUpdateAccessibleRoleWithGroupAsync(role, groupName)).Returns(default);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GroupAsync(groupName, new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        [Fact]
        public async Task GroupAsync_WhenClear_ThenReturnsSuccessEmbed()
        {
            var role = CreateFakeRole(ARoleId, ARoleName);
            A.CallTo(() => _accessibleRoleRepository.ClearGroupFromAccessibleRoleAsync(role)).Returns(default);

            var result = (TaylorBotEmbedResult)await _accessibleRolesModule.GroupAsync(new AccessibleGroupName("clear"), new RoleNotEveryoneArgument<IRole>(role));

            result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
        }

        private IRole CreateFakeRole(ulong id, string name)
        {
            var role = A.Fake<IRole>(o => o.Strict());
            A.CallTo(() => role.Id).Returns(id);
            A.CallTo(() => role.Mention).Returns(MentionUtils.MentionRole(id));
            A.CallTo(() => role.Name).Returns(name);
            return role;
        }
    }
}
