using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet;

public partial class CommandExecutedHandler(
    ILogger<CommandExecutedHandler> logger,
    IOngoingCommandRepository ongoingCommandRepository,
    IIgnoredUserRepository ignoredUserRepository,
    PageMessageReactionsHandler pageMessageReactionsHandler,
    UserNotIgnoredPrecondition userNotIgnoredPrecondition,
    TimeProvider timeProvider)
{
    public async Task OnCommandExecutedAsync(Optional<CommandInfo> optCommandInfo, ICommandContext context, IResult result)
    {
        var commandContext = (ITaylorBotCommandContext)context;

        if (result.Error != CommandError.UnknownCommand)
        {
            LogUserUsedCommand(
                context.User.FormatLog(),
                context.Message.Content.Replace("\n", "\\n", StringComparison.InvariantCulture),
                context.Channel.FormatLog());

            if (result.IsSuccess)
            {
                var innerResult = ((TaylorBotResult)result).Result;
                switch (innerResult)
                {
                    case EmbedResult embedResult:
                        await context.Channel.SendMessageAsync(
                            messageReference: new(context.Message.Id),
                            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                            embed: embedResult.Embed
                        );
                        break;

                    case PageMessageResult pageResult:
                        using (var sentPageMessage = await pageResult.PageMessage.SendAsync(context.User, context.Message))
                        {
                            await sentPageMessage.SendReactionsAsync(pageMessageReactionsHandler, logger);
                        }
                        break;

                    case RateLimitedResult rateLimited:
                        var now = timeProvider.GetUtcNow();
                        var description =
                            $"""
                            You have exceeded the '{rateLimited.FriendlyLimitName}' daily limit (**{rateLimited.Limit}**). 😕
                            This limit will reset **{now.Date.AddDays(1).Humanize(culture: TaylorBotCulture.Culture)}**.
                            """;

                        if (rateLimited.Uses < rateLimited.Limit + 6)
                        {
                            description =
                                $"""
                                {description}
                                **Stop trying to perform this action or you will be ignored.**
                                """;
                        }
                        else
                        {
                            var ignoreTime = TimeSpan.FromDays(5);

                            description =
                                $"""
                                {description}
                                You won't stop despite being warned, **I think you are a bot and will ignore you for {ignoreTime.Humanize(culture: TaylorBotCulture.Culture)}.**
                                """;

                            await ignoredUserRepository.IgnoreUntilAsync(new(context.User), now + ignoreTime);
                        }

                        await context.Channel.SendMessageAsync(
                            messageReference: new(context.Message.Id),
                            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                            embed: EmbedFactory.CreateError(description)
                        );
                        break;

                    case PreconditionFailed failed:
                        LogPreconditionFailure(commandContext.User.FormatLog(), failed.PrivateReason);
                        if (!failed.UserReason.HideInPrefixCommands)
                        {
                            await context.Channel.SendMessageAsync(
                                messageReference: new(context.Message.Id),
                                allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                                embed: EmbedFactory.CreateError(failed.UserReason.Reason));
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
                                LogUnhandledExceptionInCommand(executeResult.Exception);
                                break;

                            default:
                                LogUnhandledErrorInCommand(executeResult.Exception, result.Error, result.ErrorReason);
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
                            DiscordNetContextMapper.MapToRunContext(commandContext, new())
                        );

                        if (runResult is not PreconditionFailed failed || !failed.UserReason.HideInPrefixCommands)
                        {
                            await context.Channel.SendMessageAsync(
                                messageReference: new(context.Message.Id),
                                allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                                embed: EmbedFactory.CreateError(
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
                            LogPreconditionFailure(commandContext.User.FormatLog(), failed.PrivateReason);
                        }
                        break;

                    default:
                        LogUnhandledError(result.Error, result.ErrorReason);
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
            await ongoingCommandRepository.RemoveOngoingCommandAsync(new(context.User), commandContext.RunContext.OnGoing.OnGoingCommandAddedToPool);
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

    [LoggerMessage(Level = LogLevel.Information, Message = "{User} used '{MessageContent}' in {Channel}")]
    private partial void LogUserUsedCommand(string user, string messageContent, string channel);

    [LoggerMessage(Level = LogLevel.Information, Message = "{User} precondition failure: {PrivateReason}.")]
    private partial void LogPreconditionFailure(string user, string privateReason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in command:")]
    private partial void LogUnhandledExceptionInCommand(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error in command - {Error}, {ErrorReason}:")]
    private partial void LogUnhandledErrorInCommand(Exception exception, CommandError? error, string? errorReason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error in command - {Error}, {ErrorReason}")]
    private partial void LogUnhandledError(CommandError? error, string? errorReason);
}
