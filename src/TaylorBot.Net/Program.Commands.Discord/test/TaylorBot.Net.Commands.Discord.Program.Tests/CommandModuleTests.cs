using Discord;
using FakeItEasy;
using FluentAssertions;
using TaylorBot.Net.Commands.Discord.Program.Modules.CommandDisabling;
using TaylorBot.Net.Commands.Discord.Program.Tests.Helpers;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using Xunit;

namespace TaylorBot.Net.Commands.Discord.Program.Tests;

public class CommandModuleTests
{
    private readonly IUser _commandUser = A.Fake<IUser>();
    private readonly ITaylorBotCommandContext _commandContext = A.Fake<ITaylorBotCommandContext>();
    private readonly IMessageChannel _channel = A.Fake<ITextChannel>();
    private readonly IDisabledCommandRepository _disabledCommandRepository = A.Fake<IDisabledCommandRepository>(o => o.Strict());
    private readonly CommandModule _commandModule;

    public CommandModuleTests()
    {
        _commandModule = new CommandModule(new SimpleCommandRunner(), _disabledCommandRepository, new(CommandUtils.Mentioner));
        _commandModule.SetContext(_commandContext);
        A.CallTo(() => _commandContext.IsTestEnv).Returns(true);
        A.CallTo(() => _commandContext.User).Returns(_commandUser);
        A.CallTo(() => _commandContext.Channel).Returns(_channel);
    }

    [Fact]
    public async Task EnableGlobalAsync_ThenReturnsSuccessEmbed()
    {
        const string CommandName = "avatar";
        A.CallTo(() => _disabledCommandRepository.EnableGloballyAsync(CommandName)).Returns(default);
        var command = new ICommandRepository.Command(Name: CommandName);

        var result = (await _commandModule.EnableGlobalAsync(command)).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }

    [Fact]
    public async Task DisableGlobalAsync_WhenGuarded_ThenReturnsErrorEmbed()
    {
        const string CommandName = "command enable-globally";
        const string DisabledMesssage = "The command is down for maintenance.";
        A.CallTo(() => _disabledCommandRepository.DisableGloballyAsync(CommandName, DisabledMesssage)).Returns(DisabledMesssage);
        var command = new ICommandRepository.Command(Name: CommandName);

        var result = (await _commandModule.DisableGlobalAsync(command, DisabledMesssage)).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.ErrorColor);
    }

    [Fact]
    public async Task DisableGlobalAsync_ThenReturnsSuccessEmbed()
    {
        const string CommandName = "avatar";
        const string DisabledMesssage = "The command is down for maintenance.";
        A.CallTo(() => _disabledCommandRepository.DisableGloballyAsync(CommandName, DisabledMesssage)).Returns(DisabledMesssage);
        var command = new ICommandRepository.Command(Name: CommandName);

        var result = (await _commandModule.DisableGlobalAsync(command, DisabledMesssage)).GetResult<EmbedResult>();

        result.Embed.Color.Should().Be(TaylorBotColors.SuccessColor);
    }
}
