using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet;

public class CommandExecutedHandler
{
    private readonly ILogger<CommandExecutedHandler> _logger;
    private readonly IOngoingCommandRepository _ongoingCommandRepository;
    private readonly ICommandUsageRepository _commandUsageRepository;
    private readonly IIgnoredUserRepository _ignoredUserRepository;
    private readonly PageMessageReactionsHandler _pageMessageReactionsHandler;
    private readonly UserNotIgnoredPrecondition _userNotIgnoredPrecondition;

    public CommandExecutedHandler(
        ILogger<CommandExecutedHandler> logger,
        IOngoingCommandRepository ongoingCommandRepository,
        ICommandUsageRepository commandUsageRepository,
        IIgnoredUserRepository ignoredUserRepository,
        PageMessageReactionsHandler pageMessageReactionsHandler,
        UserNotIgnoredPrecondition userNotIgnoredPrecondition
    )
    {
        _logger = logger;
        _ongoingCommandRepository = ongoingCommandRepository;
        _commandUsageRepository = commandUsageRepository;
        _ignoredUserRepository = ignoredUserRepository;
        _pageMessageReactionsHandler = pageMessageReactionsHandler;
        _userNotIgnoredPrecondition = userNotIgnoredPrecondition;
    }

    public async Task OnCommandExecutedAsync(Optional<CommandInfo> optCommandInfo, ICommandContext context, IResult result)
    {
        var commandContext = (ITaylorBotCommandContext)context;

        if (result.Error != CommandError.UnknownCommand)
        {
            _logger.LogInformation($"{context.User.FormatLog()} used '{context.Message.Content.Replace("\n", "\\n")}' in {context.Channel.FormatLog()}");

            if (result.IsSuccess)
            {
                switch (((TaylorBotResult)result).Result)
                {
                    case EmbedResult embedResult:
                        // Check for slash command-like errors and use message reply
                        if (embedResult.Embed.Color == TaylorBotColors.ErrorColor && !embedResult.Embed.Author.HasValue)
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
                        await sentPageMessage.SendReactionsAsync(_pageMessageReactionsHandler, _logger);
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

                            await _ignoredUserRepository.IgnoreUntilAsync(context.User, DateTimeOffset.Now + ignoreTime);
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
                        _logger.LogInformation($"{commandContext.User.FormatLog()} precondition failure: {failed.PrivateReason}.");
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
                        throw new InvalidOperationException($"Unexpected command success result: {result.GetType()}");
                }
                if (optCommandInfo.IsSpecified)
                    _commandUsageRepository.QueueIncrementSuccessfulUseCount(optCommandInfo.Value.Aliases[0]);
            }
            else
            {
                switch (result)
                {
                    case ExecuteResult executeResult:
                        switch (result.Error)
                        {
                            case CommandError.Exception:
                                _logger.LogError(executeResult.Exception, "Unhandled exception in command:");
                                break;

                            default:
                                _logger.LogError(executeResult.Exception, $"Unhandled error in command - {result.Error}, {result.ErrorReason}:");
                                break;
                        }
                        await context.Channel.SendMessageAsync(
                            messageReference: new(context.Message.Id),
                            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
                            embed: CreateUnknownErrorEmbed()
                        );
                        if (optCommandInfo.IsSpecified)
                            _commandUsageRepository.QueueIncrementUnhandledErrorCount(optCommandInfo.Value.Aliases[0]);
                        break;

                    case ParseResult parseResult:
                        // Preconditions have not been executed, we must make sure the user is not ignored.
                        var runResult = await _userNotIgnoredPrecondition.CanRunAsync(
                            new Command(DiscordNetContextMapper.MapToCommandMetadata(commandContext), () => new()),
                            DiscordNetContextMapper.MapToRunContext(commandContext)
                        );

                        if (runResult is not PreconditionFailed failed || !failed.UserReason.HideInPrefixCommands)
                        {
                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(TaylorBotColors.ErrorColor)
                                .WithDescription(string.Join('\n',
                                    $"{context.User.Mention} Format: `{commandContext.GetUsage(optCommandInfo.Value)}`",
                                    parseResult.ErrorParameter != null ?
                                        $"`<{parseResult.ErrorParameter.Name}>`: {parseResult.ErrorReason}" :
                                        parseResult.ErrorReason
                                ))
                            .Build());
                        }
                        else
                        {
                            _logger.LogInformation($"{commandContext.User.FormatLog()} precondition failure: {failed.PrivateReason}.");
                        }
                        break;

                    default:
                        _logger.LogError($"Unhandled error in command - {result.Error}, {result.ErrorReason}");
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
            await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, commandContext.RunContext.OnGoing.OnGoingCommandAddedToPool);
        }
    }

    private static Embed CreateUnknownErrorEmbed()
    {
        return new EmbedBuilder()
            .WithColor(TaylorBotColors.ErrorColor)
            .WithDescription($"Oops, an unknown command error occurred. Sorry about that. 😕")
        .Build();
    }
}
