using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet;

public class CommandExecutedHandler(
    ILogger<CommandExecutedHandler> logger,
    IOngoingCommandRepository ongoingCommandRepository,
    IIgnoredUserRepository ignoredUserRepository,
    PageMessageReactionsHandler pageMessageReactionsHandler,
    UserNotIgnoredPrecondition userNotIgnoredPrecondition)
{
    public async Task OnCommandExecutedAsync(Optional<CommandInfo> optCommandInfo, ICommandContext context, IResult result)
    {
        var commandContext = (ITaylorBotCommandContext)context;

        if (result.Error != CommandError.UnknownCommand)
        {
            logger.LogInformation("{User} used '{MessageContent}' in {Channel}", context.User.FormatLog(), context.Message.Content.Replace("\n", "\\n"), context.Channel.FormatLog());

            if (result.IsSuccess)
            {
                var innerResult = ((TaylorBotResult)result).Result;
                switch (innerResult)
                {
                    case EmbedResult embedResult:
                        // Check for slash command-like errors and use message reply
                        if ((embedResult.Embed.Color == TaylorBotColors.ErrorColor && !embedResult.Embed.Author.HasValue)
                            || embedResult.PrefixCommandReply)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: embedResult.Embed,
                                messageReference: new(context.Message.Id),
                                allowedMentions: new AllowedMentions { MentionRepliedUser = false }
                            );
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(embed: embedResult.Embed);
                        }
                        break;

                    case PageMessageResult pageResult:
                        var sentPageMessage = await pageResult.PageMessage.SendAsync(context.User, context.Channel);
                        await sentPageMessage.SendReactionsAsync(pageMessageReactionsHandler, logger);
                        break;

                    case RateLimitedResult rateLimited:
                        var baseDescriptionLines = new[] {
                            $"You have exceeded the '{rateLimited.FriendlyLimitName}' daily limit (**{rateLimited.Limit}**). 😕",
                            $"This limit will reset **{DateTimeOffset.UtcNow.Date.AddDays(1).Humanize(culture: TaylorBotCulture.Culture)}**."
                        };

                        if (rateLimited.Uses < rateLimited.Limit + 6)
                        {
                            baseDescriptionLines = baseDescriptionLines
                                .Append("**Stop trying to perform this action or you will be ignored.**")
                                .ToArray();
                        }
                        else
                        {
                            var ignoreTime = TimeSpan.FromDays(5);

                            baseDescriptionLines = baseDescriptionLines
                                .Append($"You won't stop despite being warned, **I think you are a bot and will ignore you for {ignoreTime.Humanize(culture: TaylorBotCulture.Culture)}.**")
                                .ToArray();

                            await ignoredUserRepository.IgnoreUntilAsync(context.User, DateTimeOffset.Now + ignoreTime);
                        }

                        await context.Channel.SendMessageAsync(
                            messageReference: new(context.Message.Id),
                            embed: new EmbedBuilder()
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n', baseDescriptionLines))
                            .Build()
                        );
                        break;

                    case PreconditionFailed failed:
                        logger.LogInformation("{User} precondition failure: {PrivateReason}.", commandContext.User.FormatLog(), failed.PrivateReason);
                        if (!failed.UserReason.HideInPrefixCommands)
                        {
                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription($"{context.User.Mention} {failed.UserReason.Reason}")
                            .Build());
                        }
                        break;

                    case EmptyResult _:
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected command success result: {innerResult.GetType()}");
                }
            }
            else
            {
                switch (result)
                {
                    case ExecuteResult executeResult:
                        switch (result.Error)
                        {
                            case CommandError.Exception:
                                logger.LogError(executeResult.Exception, "Unhandled exception in command:");
                                break;

                            default:
                                logger.LogError(executeResult.Exception, "Unhandled error in command - {Error}, {ErrorReason}:", result.Error, result.ErrorReason);
                                break;
                        }
                        commandContext.Activity.Value.SetError(executeResult.Exception);

                        await context.Channel.SendMessageAsync(
                            messageReference: new(context.Message.Id),
                            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                            embed: CreateUnknownErrorEmbed()
                        );
                        break;

                    case ParseResult parseResult:
                        // Preconditions have not been executed, we must make sure the user is not ignored.
                        var runResult = await userNotIgnoredPrecondition.CanRunAsync(
                            new Command(DiscordNetContextMapper.MapToCommandMetadata(commandContext), () => new()),
                            DiscordNetContextMapper.MapToRunContext(commandContext)
                        );

                        if (runResult is not PreconditionFailed failed || !failed.UserReason.HideInPrefixCommands)
                        {
                            await context.Channel.SendMessageAsync(embed: EmbedFactory.CreateError(
                                $"""
                                {context.User.Mention} Format: `{commandContext.GetUsage(optCommandInfo.Value)}`
                                {(parseResult.ErrorParameter != null
                                    ? $"`<{parseResult.ErrorParameter.Name}>`: {parseResult.ErrorReason}"
                                    : parseResult.ErrorReason)}
                                """
                            ));
                        }
                        else
                        {
                            logger.LogInformation("{User} precondition failure: {PrivateReason}.", commandContext.User.FormatLog(), failed.PrivateReason);
                        }
                        break;

                    default:
                        logger.LogError("Unhandled error in command - {Error}, {ErrorReason}", result.Error, result.ErrorReason);
                        commandContext.Activity.Value.SetError();

                        await context.Channel.SendMessageAsync(
                            messageReference: new(context.Message.Id),
                            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                            embed: CreateUnknownErrorEmbed()
                        );
                        break;
                }
            }
        }

        if (commandContext.RunContext?.OnGoing.OnGoingCommandAddedToPool != null)
        {
            await ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, commandContext.RunContext.OnGoing.OnGoingCommandAddedToPool);
        }

        if (commandContext.Activity.IsValueCreated)
        {
            commandContext.Activity.Value.Dispose();
        }
    }

    private static Embed CreateUnknownErrorEmbed()
    {
        return EmbedFactory.CreateError($"Oops, an unknown command error occurred. Sorry about that. 😕");
    }
}
