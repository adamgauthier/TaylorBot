using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands
{
    public class CommandExecutedHandler
    {
        private readonly ILogger<CommandExecutedHandler> _logger;
        private readonly IOngoingCommandRepository _ongoingCommandRepository;
        private readonly ICommandUsageRepository _commandUsageRepository;

        public CommandExecutedHandler(ILogger<CommandExecutedHandler> logger, IOngoingCommandRepository ongoingCommandRepository, ICommandUsageRepository commandUsageRepository)
        {
            _logger = logger;
            _ongoingCommandRepository = ongoingCommandRepository;
            _commandUsageRepository = commandUsageRepository;
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> optCommandInfo, ICommandContext context, IResult result)
        {
            var commandContext = (TaylorBotShardedCommandContext)context;

            if (commandContext.OnGoingCommandAddedToPool != null)
            {
                await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, commandContext.OnGoingCommandAddedToPool);
            }

            if (result.Error == CommandError.UnknownCommand)
            {
                return;
            }

            _logger.LogInformation(LogString.From(
                $"{context.User.FormatLog()} used '{context.Message.Content.Replace("\n", "\\n")}' in {context.Channel.FormatLog()}"
            ));

            if (result.IsSuccess)
            {
                switch (result)
                {
                    case TaylorBotEmbedResult embedResult:
                        await context.Channel.SendMessageAsync(embed: embedResult.Embed);
                        break;

                    case TaylorBotEmptyResult emptyResult:
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected command success result: {result.GetType()}");
                }
                _commandUsageRepository.AddSuccessfulUseCount(optCommandInfo.Value);
                return;
            }

            switch (result)
            {
                case ExecuteResult executeResult:
                    switch (result.Error)
                    {
                        case CommandError.Exception:
                            _logger.LogError(executeResult.Exception, LogString.From("Unhandled exception in command:"));
                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription($"{context.User.Mention} Oops, an unknown command error occurred. Sorry about that. 😕")
                            .Build());
                            break;

                        default:
                            _logger.LogError(executeResult.Exception, LogString.From($"Unhandled error in command - {result.Error}, {result.ErrorReason}:"));
                            break;
                    }
                    _commandUsageRepository.AddUnhandledErrorCount(optCommandInfo.Value);
                    break;

                case ParseResult parseResult:
                    var cmd = optCommandInfo.Value;
                    await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(string.Join('\n',
                            $"{context.User.Mention} Format: `{commandContext.GetUsage(cmd)}`",
                            $"`<{parseResult.ErrorParameter.Name}>`: {parseResult.ErrorReason}"
                        ))
                    .Build());
                    break;

                case TaylorBotPreconditionResult preconditionResult:
                    _logger.LogInformation(LogString.From(
                        $"{commandContext.User.FormatLog()} precondition failure: {preconditionResult.ErrorReason}."
                    ));
                    if (preconditionResult.UserReason != null)
                    {
                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithColor(TaylorBotColors.ErrorColor)
                            .WithDescription($"{context.User.Mention} {preconditionResult.UserReason}")
                        .Build());
                    }
                    break;

                default:
                    _logger.LogError(LogString.From($"Unhandled error in command - {result.Error}, {result.ErrorReason}"));
                    break;
            }
        }
    }
}
