using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands
{
    public class CommandExecutedHandler
    {
        private readonly ILogger<CommandExecutedHandler> logger;

        public CommandExecutedHandler(ILogger<CommandExecutedHandler> logger)
        {
            this.logger = logger;
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> optCommandInfo, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
                return;

            var commandContext = (TaylorBotShardedCommandContext)context;

            switch (result)
            {
                case ExecuteResult executeResult:
                    switch (result.Error)
                    {
                        case CommandError.Exception:
                            logger.LogError(executeResult.Exception, LogString.From("Unhandled exception in command:"));
                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription($"{context.User.Mention} Oops, an unknown command error occurred. Sorry about that. 😕")
                            .Build());
                            break;

                        default:
                            logger.LogError(executeResult.Exception, LogString.From($"Unhandled error in command - {result.Error}, {result.ErrorReason}:"));
                            break;
                    }
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

                default:
                    logger.LogError(LogString.From($"Unhandled error in command - {result.Error}, {result.ErrorReason}."));
                    break;
            }
        }
    }
}
