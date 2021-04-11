using Discord;
using Discord.Commands;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using Xunit;

namespace TaylorBot.Net.Commands.Types.Tests
{
    public class CommandExecutedHandlerTests
    {
        private readonly ILogger<CommandExecutedHandler> _logger = A.Fake<ILogger<CommandExecutedHandler>>(o => o.Strict());
        private readonly IOngoingCommandRepository _ongoingCommandRepository = A.Fake<IOngoingCommandRepository>(o => o.Strict());
        private readonly ICommandUsageRepository _commandUsageRepository = A.Fake<ICommandUsageRepository>(o => o.Strict());
        private readonly IIgnoredUserRepository _ignoredUserRepository = A.Fake<IIgnoredUserRepository>(o => o.Strict());
        private readonly PageMessageReactionsHandler _pageMessageReactionsHandler = new PageMessageReactionsHandler();

        private readonly CommandExecutedHandler _commandExecutedHandler;

        public CommandExecutedHandlerTests()
        {
            _commandExecutedHandler = new CommandExecutedHandler(
                _logger, _ongoingCommandRepository, _commandUsageRepository, _ignoredUserRepository, _pageMessageReactionsHandler
            );
        }

        [Fact]
        public async Task OnCommandExecutedAsync_WhenUnknownCommand_ThenNoLog()
        {
            var commandContext = A.Fake<ITaylorBotCommandContext>(o => o.Strict());
            A.CallTo(() => commandContext.OnGoingCommandAddedToPool).Returns(null);
            var result = A.Fake<IResult>(o => o.Strict());
            A.CallTo(() => result.Error).Returns(CommandError.UnknownCommand);

            await _commandExecutedHandler.OnCommandExecutedAsync(Optional.Create<CommandInfo>(), commandContext, result);

            A.CallTo(_logger).Where(call => call.Method.Name == nameof(ILogger.Log)).MustNotHaveHappened();
        }
    }
}
