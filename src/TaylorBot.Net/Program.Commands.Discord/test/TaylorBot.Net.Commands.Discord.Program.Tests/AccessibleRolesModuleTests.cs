using Discord;
using Discord.Net;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.AccessibleRoles.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.StringMappers;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class AccessibleRolesModuleTests
{
    private const ulong ARoleId = 13;
    private const string ARoleName = "Regulars";

    private readonly IGuildUser _commandUser = A.Fake<IGuildUser>();
    private readonly IUserMessage _message = A.Fake<IUserMessage>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly IGuild _guild = A.Fake<IGuild>(o => o.Strict());
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IAccessibleRoleRepository _accessibleRoleRepository = A.Fake<IAccessibleRoleRepository>(o => o.Strict());
    private readonly AccessibleRolesModule _accessibleRolesModule;

    public AccessibleRolesModuleTests()
    {
        ServiceCollection services = new();
        services.AddSingleton(CommandUtils.Mentioner);
        services.AddTransient<PermissionStringMapper>();
        services.AddTransient<TaylorBotOwnerPrecondition>();
        services.AddTransient<InGuildPrecondition.Factory>();
        var serviceProvider = services.BuildServiceProvider();

        _accessibleRolesModule = new AccessibleRolesModule(new SimpleCommandRunner(), _accessibleRoleRepository, new(serviceProvider), new(serviceProvider));
        _accessibleRolesModule.SetContext(_commandContext);

        A.CallTo(() => _guild.Id).Returns(123u);
        A.CallTo(() => _commandContext.IsTestEnv).Returns(true);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
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
        A.CallTo(() => _commandUser.RoleIds).Returns([RoleId]);

        var result = (await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task GetAsync_WhenRoleIsNotAccessible_ThenReturnsErrorEmbed()
    {
        const ulong RoleId = 1989;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([]);
        A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(null);

        var result = (await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task GetAsync_WhenRoleIsAccessibleWithNoGroup_ThenReturnsSuccessEmbed()
    {
        const ulong RoleId = 1989;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([]);
        A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(new AccessibleRoleWithGroup(Group: null));
        A.CallTo(() => _commandUser.AddRoleAsync(role, A<RequestOptions>.Ignored)).Returns(Task.CompletedTask);

        var result = (await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task GetAsync_WhenRoleIsAccessibleInGroupThatUserAlreadyHas_ThenReturnsErrorEmbed()
    {
        const ulong RoleId = 1989;
        const ulong AnotherRoleId = 1322;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([AnotherRoleId]);
        A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(new AccessibleRoleWithGroup(
            Group: new AccessibleRoleGroup(Name: "regions", OtherRoles: [new SnowflakeId(AnotherRoleId)])
        ));
        A.CallTo(() => _commandUser.AddRoleAsync(role, A<RequestOptions>.Ignored)).Returns(Task.CompletedTask);

        var result = (await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task GetAsync_WhenRoleIsAccessibleButDiscordForbidsAddingRole_ThenReturnsErrorEmbed()
    {
        const ulong RoleId = 1989;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([]);
        A.CallTo(() => _accessibleRoleRepository.GetAccessibleRoleAsync(role)).Returns(new AccessibleRoleWithGroup(Group: null));
        A.CallTo(() => _commandUser.AddRoleAsync(role, A<RequestOptions>.Ignored)).Throws(new HttpException(HttpStatusCode.Forbidden, A.Fake<IRequest>()));

        var result = (await _accessibleRolesModule.GetAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task DropAsync_WhenUserDoesntHaveRole_ThenReturnsErrorEmbed()
    {
        const ulong RoleId = 1989;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([]);

        var result = (await _accessibleRolesModule.DropAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task DropAsync_WhenRoleIsNotAccessible_ThenReturnsErrorEmbed()
    {
        const ulong RoleId = 1989;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([RoleId]);
        A.CallTo(() => _accessibleRoleRepository.IsRoleAccessibleAsync(role)).Returns(false);

        var result = (await _accessibleRolesModule.DropAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task DropAsync_WhenRoleIsAccessible_ThenReturnsSuccessEmbed()
    {
        const ulong RoleId = 1989;
        var role = CreateFakeRole(RoleId, ARoleName);
        A.CallTo(() => _commandUser.RoleIds).Returns([RoleId]);
        A.CallTo(() => _accessibleRoleRepository.IsRoleAccessibleAsync(role)).Returns(true);
        A.CallTo(() => _commandUser.RemoveRoleAsync(role, A<RequestOptions>.Ignored)).Returns(Task.CompletedTask);

        var result = (await _accessibleRolesModule.DropAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task AddAsync_ThenReturnsSuccessEmbed()
    {
        var role = CreateFakeRole(ARoleId, ARoleName);
        A.CallTo(() => _accessibleRoleRepository.AddAccessibleRoleAsync(role)).Returns(default);

        var result = (await _accessibleRolesModule.AddAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task RemoveAsync_ThenReturnsSuccessEmbed()
    {
        var role = CreateFakeRole(ARoleId, ARoleName);
        A.CallTo(() => _accessibleRoleRepository.RemoveAccessibleRoleAsync(role)).Returns(default);

        var result = (await _accessibleRolesModule.RemoveAsync(new RoleArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task GroupAsync_ThenReturnsSuccessEmbed()
    {
        AccessibleGroupName groupName = new("regions");
        var role = CreateFakeRole(ARoleId, ARoleName);
        A.CallTo(() => _accessibleRoleRepository.AddOrUpdateAccessibleRoleWithGroupAsync(role, groupName)).Returns(default);

        var result = (await _accessibleRolesModule.GroupAsync(groupName, new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task GroupAsync_WhenClear_ThenReturnsSuccessEmbed()
    {
        var role = CreateFakeRole(ARoleId, ARoleName);
        A.CallTo(() => _accessibleRoleRepository.ClearGroupFromAccessibleRoleAsync(role)).Returns(default);

        var result = (await _accessibleRolesModule.GroupAsync(new AccessibleGroupName("clear"), new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    private static IRole CreateFakeRole(ulong id, string name)
    {
        var role = A.Fake<IRole>(o => o.Strict());
        A.CallTo(() => role.Id).Returns(id);
        A.CallTo(() => role.Mention).Returns(MentionUtils.MentionRole(id));
        A.CallTo(() => role.Name).Returns(name);
        return role;
    }
}
