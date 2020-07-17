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
            var commandContext = (ITaylorBotCommandContext)context;

            if (result.Error != CommandError.UnknownCommand)
            {
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
                }
                else
                {
                    switch (result)
                    {
                        case ExecuteResult executeResult:
                            switch (result.Error)
                            {
                                case CommandError.Exception:
                                    _logger.LogError(executeResult.Exception, LogString.From("Unhandled exception in command:"));
                                    break;

                                default:
                                    _logger.LogError(executeResult.Exception, LogString.From($"Unhandled error in command - {result.Error}, {result.ErrorReason}:"));
                                    break;
                            }
                            await context.Channel.SendMessageAsync(embed: CreateUnknownErrorEmbed(context.User));
                            _commandUsageRepository.AddUnhandledErrorCount(optCommandInfo.Value);
                            break;

                        case ParseResult parseResult:
                            var cmd = optCommandInfo.Value;
                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n',
                                    $"{context.User.Mention} Format: `{commandContext.GetUsage(cmd)}`",
                                    parseResult.ErrorParameter != null ?
                                        $"`<{parseResult.ErrorParameter.Name}>`: {parseResult.ErrorReason}" :
                                        parseResult.ErrorReason
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
                            await context.Channel.SendMessageAsync(embed: CreateUnknownErrorEmbed(context.User));
                            break;
                    }
                }
            }

            if (commandContext.OnGoingCommandAddedToPool != null)
            {
                await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, commandContext.OnGoingCommandAddedToPool);
            }
        }

        private Embed CreateUnknownErrorEmbed(IUser user)
        {
            return new EmbedBuilder()
                .WithColor(TaylorBotColors.ErrorColor)
                .WithDescription($"{user.Mention} Oops, an unknown command error occurred. Sorry about that. 😕")
            .Build();
        }
    }
}
