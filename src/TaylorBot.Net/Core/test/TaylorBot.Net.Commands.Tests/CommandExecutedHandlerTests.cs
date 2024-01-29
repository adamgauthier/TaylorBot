using Discord;
using Discord.Commands;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.Username;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests;

public class CommandExecutedHandlerTests
{
    private readonly ILogger<CommandExecutedHandler> _logger = A.Fake<ILogger<CommandExecutedHandler>>(o => o.Strict());
    private readonly IOngoingCommandRepository _ongoingCommandRepository = A.Fake<IOngoingCommandRepository>(o => o.Strict());
    private readonly IIgnoredUserRepository _ignoredUserRepository = A.Fake<IIgnoredUserRepository>(o => o.Strict());
    private readonly PageMessageReactionsHandler _pageMessageReactionsHandler = new();
    private readonly UserNotIgnoredPrecondition _userNotIgnoredPrecondition = new(
        A.Fake<IIgnoredUserRepository>(o => o.Strict()),
        new UsernameTrackerDomainService(A.Fake<ILogger<UsernameTrackerDomainService>>(o => o.Strict()), A.Fake<IUsernameRepository>(o => o.Strict()))
    );

    private readonly CommandExecutedHandler _commandExecutedHandler;

    public CommandExecutedHandlerTests()
    {
        _commandExecutedHandler = new(
            _logger, _ongoingCommandRepository, _ignoredUserRepository, _pageMessageReactionsHandler, _userNotIgnoredPrecondition
        );
    }

    [Fact]
    public async Task OnCommandExecutedAsync_WhenUnknownCommand_ThenNoLog()
    {
        var commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
        A.CallTo(() => commandContext.RunContext).Returns(null);
        A.CallTo(() => commandContext.Activity).Returns(new(new CommandActivity(null)));
        var result = A.Fake<IResult>(o => o.Strict());
        A.CallTo(() => result.Error).Returns(CommandError.UnknownCommand);

        await _commandExecutedHandler.OnCommandExecutedAsync(Optional.Create<CommandInfo>(), commandContext, result);

        A.CallTo(_logger).Where(call => call.Method.Name == nameof(ILogger.Log)).MustNotHaveHappened();
    }
}
