using Discord;
using Discord.Net;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Jail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.StringMappers;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class JailModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly IGuild _guild = A.Fake<IGuild>(o => o.Strict());
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IJailRepository _jailRepository = A.Fake<IJailRepository>(o => o.Strict());
    private readonly IModChannelLogger _modLogChannelLogger = A.Fake<IModChannelLogger>();
    private readonly JailModule _jailModule;

    public JailModuleTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(CommandUtils.Mentioner);
        services.AddTransient<PermissionStringMapper>();
        services.AddTransient<TaylorBotOwnerPrecondition>();
        services.AddTransient<InGuildPrecondition.Factory>();
        var serviceProvider = services.BuildServiceProvider();

        _jailModule = new JailModule(new SimpleCommandRunner(), _jailRepository, _modLogChannelLogger, new(serviceProvider), new(serviceProvider));
        _jailModule.SetContext(_commandContext);

        A.CallTo(() => _guild.Id).Returns(123u);
        A.CallTo(() => _commandContext.IsTestEnv).Returns(true);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => _commandContext.Guild).Returns(_guild);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
        A.CallTo(() => _commandContext.CommandPrefix).Returns("!");
        A.CallTo(() => _modLogChannelLogger.CreateResultEmbed(A<RunContext>.Ignored, A<bool>.Ignored, A<string>.Ignored)).ReturnsLazily(call => EmbedFactory.CreateSuccess(call.Arguments.Get<string>(2)!));
    }

    [Fact]
    public async Task JailAsync_WhenNoJailRoleSet_ThenReturnsErrorEmbed()
    {
        A.CallTo(() => _jailRepository.GetJailRoleAsync(_guild)).Returns(null);

        var result = (await _jailModule.JailAsync(A.Fake<IMentionedUserNotAuthorOrClient<IGuildUser>>(o => o.Strict()))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task JailAsync_WhenJailRoleDeleted_ThenReturnsErrorEmbed()
    {
        const ulong AnId = 1;
        A.CallTo(() => _jailRepository.GetJailRoleAsync(_guild)).Returns(new JailRole(new SnowflakeId(AnId)));
        A.CallTo(() => _guild.GetRole(AnId)).Returns(null!);

        var result = (await _jailModule.JailAsync(A.Fake<IMentionedUserNotAuthorOrClient<IGuildUser>>(o => o.Strict()))).GetResult<EmbedResult>();

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

    private static IMentionedUserNotAuthorOrClient<IGuildUser> CreateMentionedUserNotClient(IGuildUser user)
    {
        var mentionedUser = A.Fake<IMentionedUserNotAuthorOrClient<IGuildUser>>(o => o.Strict());
        A.CallTo(() => mentionedUser.GetTrackedUserAsync()).Returns(user);
        return mentionedUser;
    }

    private static IMentionedUserNotAuthor<IGuildUser> CreateMentionedUser(IGuildUser user)
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
        A.CallTo(() => user.AddRoleAsync(jailRole, A<RequestOptions>.Ignored)).ThrowsAsync(
            new HttpException(System.Net.HttpStatusCode.Forbidden, A.Fake<IRequest>(), null, null)
        );

        var result = (await _jailModule.JailAsync(CreateMentionedUserNotClient(user))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task JailAsync_WhenJailRoleSetAndCanBeAdded_ThenReturnsSuccessEmbed()
    {
        var jailRole = SetupValidJailRole();
        var user = A.Fake<IGuildUser>();
        A.CallTo(() => user.Mention).Returns(string.Empty);
        A.CallTo(() => user.AddRoleAsync(jailRole, null)).Returns(Task.CompletedTask);

        var result = (await _jailModule.JailAsync(CreateMentionedUserNotClient(user))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task Free_WhenJailRoleSetAndCanBeRemoved_ThenReturnsSuccessEmbed()
    {
        var jailRole = SetupValidJailRole();
        var user = A.Fake<IGuildUser>();
        A.CallTo(() => user.Mention).Returns(string.Empty);
        A.CallTo(() => user.RoleIds).Returns([jailRole.Id]);
        A.CallTo(() => user.RemoveRoleAsync(jailRole, null)).Returns(Task.CompletedTask);

        var result = (await _jailModule.FreeAsync(CreateMentionedUser(user))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task SetAsync_ThenReturnsSuccessEmbed()
    {
        const ulong AnId = 1;
        var role = A.Fake<IRole>();
        A.CallTo(() => role.Id).Returns(AnId);
        A.CallTo(() => _jailRepository.SetJailRoleAsync(_guild, role)).Returns(default);

        var result = (await _jailModule.SetAsync(new RoleNotEveryoneArgument<IRole>(role))).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
