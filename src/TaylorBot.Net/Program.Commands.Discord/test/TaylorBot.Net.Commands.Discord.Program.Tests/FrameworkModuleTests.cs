using Discord;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaylorBot.Net.Commands.Discord.Program.Modules.Framework.Commands;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.StringMappers;
using TaylorBot.Net.Commands.Types;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class FrameworkModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly IGuild _commandGuild = A.Fake<IGuild>(o => o.Strict());
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly ICommandPrefixRepository _commandPrefixRepository = A.Fake<ICommandPrefixRepository>(o => o.Strict());
    private readonly FrameworkModule _frameworkModule;

    public FrameworkModuleTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<PermissionStringMapper>();
        services.AddTransient<TaylorBotOwnerPrecondition>();
        services.AddTransient<InGuildPrecondition.Factory>();

        _frameworkModule = new FrameworkModule(new SimpleCommandRunner(), _commandPrefixRepository, new(services.BuildServiceProvider()));
        _frameworkModule.SetContext(_commandContext);

        A.CallTo(() => _commandGuild.Id).Returns(123u);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
        A.CallTo(() => _commandContext.Guild).Returns(_commandGuild);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
    }

    [Fact]
    public async Task PrefixAsync_ThenReturnsEmbedWithNewPrefix()
    {
        var newPrefix = ".";
        A.CallTo(() => _commandPrefixRepository.ChangeGuildPrefixAsync(_commandGuild, newPrefix)).Returns(default);

        var result = (await _frameworkModule.PrefixAsync(new Word(newPrefix))).GetResult<EmbedResult>();

        result.Embed.Description.Should().Contain(newPrefix);
    }
}
