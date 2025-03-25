using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OperationResult;
using System.Text.Json;
using System.Threading.RateLimiting;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;
using static OperationResult.Helpers;
using static TaylorBot.Net.Commands.MessageResult;
using static TaylorBot.Net.Core.Client.Interaction;

namespace TaylorBot.Net.Commands.PostExecution;

public interface ISlashCommandInfo
{
    string Name { get; }
}

public record MessageCommandInfo(string Name, bool IsPrivateResponse = false) : ISlashCommandInfo;
public record ModalCommandInfo(string Name) : ISlashCommandInfo;

public interface ISlashCommand
{
    Type OptionType { get; }

    ValueTask<Command> GetCommandAsync(RunContext context, object options);

    ISlashCommandInfo Info { get; }
}

public interface ISlashCommand<T> : ISlashCommand, IKeyedSlashCommand
{
    Type ISlashCommand.OptionType => typeof(T);

    async ValueTask<Command> ISlashCommand.GetCommandAsync(RunContext context, object options) => await GetCommandAsync(context, (T)options);

    ValueTask<Command> GetCommandAsync(RunContext context, T options);
}

public interface IKeyedSlashCommand
{
    abstract static string CommandName { get; }
}

public record ApplicationCommand(ParsedInteraction Interaction, SnowflakeId Id, string Name);

public class SlashCommandHandler(
    IServiceProvider services,
    ILogger<SlashCommandHandler> logger,
    TaskExceptionLogger taskExceptionLogger,
    ICommandRunner commandRunner,
    IOngoingCommandRepository ongoingCommandRepository,
    IIgnoredUserRepository ignoredUserRepository,
    MessageComponentHandler messageComponentHandler,
    ModalInteractionHandler modalInteractionHandler,
    RunContextFactory contextFactory)
{
    private InteractionResponseClient CreateInteractionClient() => services.GetRequiredService<InteractionResponseClient>();

    public async ValueTask HandleAsync(Interaction interaction, CommandActivity activity)
    {
        var command = CreateApplicationCommand(interaction, activity);
        await HandleApplicationCommand(command, activity);
    }

    private static ApplicationCommand CreateApplicationCommand(Interaction interaction, CommandActivity activity)
    {
        var parsed = ParsedInteraction.Parse(interaction, activity);
        if (!parsed.Data.id.HasValue)
        {
            throw new ArgumentNullException(nameof(parsed.Data.id));
        }

        var stringId = parsed.Data.id.Value.GetString();
        ArgumentNullException.ThrowIfNull(stringId);

        ArgumentNullException.ThrowIfNull(parsed.Data.name);

        return new(parsed, stringId, parsed.Data.name);
    }

    private async ValueTask HandleApplicationCommand(ApplicationCommand command, CommandActivity activity)
    {
        var (commandName, options) = GetFullCommandNameAndOptions(command);
        activity.SetCommandName(commandName);
        foreach (var option in options ?? [])
        {
            activity.SetOption(option.name, $"{option.value}");
        }

        var created = TryCreateSlashCommand(commandName);
        if (created.IsSuccess)
        {
            var slashCommand = created.Value;
            if (slashCommand != null)
            {
                var context = await CreateRunContextAsync(command, slashCommand, activity);

                var result = await RunCommandAsync(slashCommand, context, options, command.Interaction.Data.resolved, activity);

                if (context.OnGoing.OnGoingCommandAddedToPool != null)
                {
                    await ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, context.OnGoing.OnGoingCommandAddedToPool);
                }

                switch (result)
                {
                    case EmbedResult embedResult:
                        if (context.WasAcknowledged)
                        {
                            await CreateInteractionClient().SendFollowupResponseAsync(command.Interaction, new(embedResult.Embed));
                        }
                        else
                        {
                            await CreateInteractionClient().SendImmediateResponseAsync(command, new(embedResult.Embed));
                        }
                        break;

                    case MessageResult messageResult:
                        IReadOnlyList<Button>? buttons = CreateAndBindButtons(command.Interaction, messageResult, context.User.Id.ToString());

                        await CreateInteractionClient().SendFollowupResponseAsync(
                            command.Interaction,
                            new(messageResult.Content, buttons)
                        );
                        break;

                    case CreateModalResult createModal:
                        var submitAction = createModal.SubmitAction;
                        if (submitAction != null)
                        {
                            createModal = createModal with { Id = $"{Guid.NewGuid():N}-{createModal.Id}" };

                            modalInteractionHandler.AddCallback(createModal.Id, new(async submit =>
                            {
                                if (submit.Interaction.UserId == context.User.Id)
                                {
                                    modalInteractionHandler.RemoveCallback(submit.CustomId.RawId);

                                    try
                                    {
                                        var result = await submitAction(submit);
                                        var buttons = CreateAndBindButtons(submit.Interaction, result, $"{context.User.Id}");
                                        await CreateInteractionClient().EditOriginalResponseAsync(submit.Interaction, message: new(result.Content, buttons, IsPrivate: createModal.IsPrivateResponse));
                                    }
                                    catch (Exception e)
                                    {
                                        logger.LogError(e, "Unhandled exception in modal submit {Id} action:", submit.Interaction.Id);
                                        await CreateInteractionClient().EditOriginalResponseAsync(submit.Interaction, message: new(
                                            EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that. 😕")
                                        ));
                                    }
                                }
                            }, createModal.IsPrivateResponse));

                            _ = Task.Run(async () => await taskExceptionLogger.LogOnError(async () =>
                            {
                                await Task.Delay(TimeSpan.FromMinutes(10));
                                modalInteractionHandler.RemoveCallback(createModal.Id);
                            }, nameof(CreateModalResult)));
                        }

                        await CreateInteractionClient().SendModalResponseAsync(command, createModal);
                        break;

                    case ParsingFailed parsingFailed:
                        if (context.WasAcknowledged)
                        {
                            await CreateInteractionClient().SendFollowupResponseAsync(command.Interaction, new(EmbedFactory.CreateError(parsingFailed.Message)));
                        }
                        else
                        {
                            await CreateInteractionClient().SendImmediateResponseAsync(command, new(new(EmbedFactory.CreateError(parsingFailed.Message)), IsPrivate: true));
                        }
                        break;

                    case PreconditionFailed preconditionFailed:
                        logger.LogInformation("{User} precondition failure: {PrivateReason}.", context.User.FormatLog(), preconditionFailed.PrivateReason);
                        if (context.WasAcknowledged)
                        {
                            await CreateInteractionClient().SendFollowupResponseAsync(command.Interaction, new(EmbedFactory.CreateError(preconditionFailed.UserReason.Reason)));
                        }
                        else
                        {
                            await CreateInteractionClient().SendImmediateResponseAsync(command, new(new(EmbedFactory.CreateError(preconditionFailed.UserReason.Reason)), IsPrivate: true));
                        }
                        break;

                    case EmptyResult _:
                        break;

                    case RateLimitedResult rateLimited:
                        var baseDescriptionLines = new[] {
                            $"You have exceeded the '{rateLimited.FriendlyLimitName}' daily limit (**{rateLimited.Limit}**). 😕",
                            $"This limit will reset **{DateTimeOffset.UtcNow.Date.AddDays(1).Humanize(culture: TaylorBotCulture.Culture)}**."
                        };

                        if (rateLimited.Uses < rateLimited.Limit + 6)
                        {
                            baseDescriptionLines =
                            [
                                .. baseDescriptionLines,
                                "**Stop trying to perform this action or all your commands will be ignored.**"
                            ];
                        }
                        else
                        {
                            var ignoreTime = TimeSpan.FromDays(5);

                            baseDescriptionLines =
                            [
                                .. baseDescriptionLines,
                                $"You won't stop despite being warned, **I think you are a bot and will ignore you for {ignoreTime.Humanize(culture: TaylorBotCulture.Culture)}.**",
                            ];

                            await ignoredUserRepository.IgnoreUntilAsync(context.User, DateTimeOffset.UtcNow + ignoreTime);
                        }

                        await CreateInteractionClient().SendFollowupResponseAsync(command.Interaction, new(EmbedFactory.CreateError(string.Join('\n', baseDescriptionLines))));
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected command result: {result.GetType()}");
                }
            }
            else
            {
                logger.LogWarning("Slash command {CommandName} not found", commandName);
            }
        }
        else
        {
            await CreateInteractionClient().SendImmediateResponseAsync(command, new(EmbedFactory.CreateError($"Oops, an unknown command error occurred. Sorry about that 😕")));
        }
    }

    private Result<ISlashCommand?> TryCreateSlashCommand(string commandName)
    {
        try
        {
            return Ok(services.GetKeyedService<ISlashCommand>(commandName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create slash command service {CommandName}", commandName);
            return Error();
        }
    }

    private async Task<RunContext> CreateRunContextAsync(ApplicationCommand command, ISlashCommand slashCommand, CommandActivity activity)
    {
        RunContext context;

        switch (slashCommand.Info)
        {
            case MessageCommandInfo info:
                {
                    await CreateInteractionClient().SendAckResponseWithLoadingMessageAsync(command, isEphemeral: info.IsPrivateResponse);

                    context = contextFactory.BuildContext(command.Interaction, activity, wasAcknowledged: true);

                    logger.LogInformation(
                        "{User} using slash command '{CommandName}' ({InteractionId}) in channel {ChannelId}{GuildInfo}",
                        context.User.FormatLog(), slashCommand.Info.Name, command.Interaction.Data.id, context.Channel.Id, context.Guild != null ? $" on {context.Guild.FormatLog()}" : ""
                    );
                    break;
                }

            case ModalCommandInfo:
                {
                    context = contextFactory.BuildContext(command.Interaction, activity, wasAcknowledged: false);

                    logger.LogInformation(
                        "{User} using modal command '{CommandName}' ({InteractionId}) in channel {ChannelId}{GuildInfo}",
                        context.User.FormatLog(), slashCommand.Info.Name, command.Interaction.Data.id, context.Channel.Id, context.Guild != null ? $" on {context.Guild.FormatLog()}" : ""
                    );
                    break;
                }

            default:
                throw new InvalidOperationException($"Unexpected SlashCommandInfo: {slashCommand.Info.GetType()}");
        }

        return context;
    }

    private const byte SubCommandOptionType = 1;
    private const byte SubCommandGroupOptionType = 2;

    private static (string name, IReadOnlyList<ApplicationCommandOption>? options) GetFullCommandNameAndOptions(ApplicationCommand command)
    {
        var options = command.Interaction.Data.options;
        if (options?.Count == 1)
        {
            var option = options[0];
            if (option.type == SubCommandOptionType)
            {
                return ($"{command.Name} {option.name}", option.options);
            }
            else if (option.type == SubCommandGroupOptionType)
            {
                var subOptions = option.options;
                if (subOptions?.Count == 1 && subOptions[0].type == SubCommandOptionType)
                {
                    return ($"{command.Name} {option.name} {subOptions[0].name}", subOptions[0].options);
                }
                else
                {
                    throw new ArgumentException("Expected sub command group's only option to be a sub command.");
                }
            }
        }

        return (command.Name, options);
    }

    private async ValueTask<ICommandResult> RunCommandAsync(ISlashCommand slashCommand, RunContext context, IReadOnlyList<ApplicationCommandOption>? options, Resolved? resolved, CommandActivity activity)
    {
        try
        {
            var parsedOptions = await ParseOptionsAsync(slashCommand, context, options, resolved);
            if (parsedOptions.Error != null)
                return parsedOptions.Error;

            var command = await slashCommand.GetCommandAsync(context, parsedOptions.Value);

            var result = await commandRunner.RunSlashCommandAsync(command, context);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled exception in slash command '{CommandName}':", slashCommand.Info.Name);
            activity.SetError(e);
            return new EmbedResult(EmbedFactory.CreateError($"Oops, an unknown command error occurred. Sorry about that 😕"));
        }
    }

    private async ValueTask<Result<object, ParsingFailed>> ParseOptionsAsync(ISlashCommand command, RunContext context, IReadOnlyList<ApplicationCommandOption>? options, Resolved? resolved)
    {
        if (command.OptionType == typeof(NoOptions))
            return new NoOptions();

        options ??= [];

        var constructorParameters = command.OptionType.GetConstructors().Single().GetParameters();

        var optionWithoutMatch = options.FirstOrDefault(o => !constructorParameters.Any(p => p.Name == o.name));
        if (optionWithoutMatch != null)
        {
            throw new InvalidOperationException($"Found no parameter mapping in '{command.OptionType}' for option '{optionWithoutMatch.name}'.");
        }

        List<object?> args = new(constructorParameters.Length);

        foreach (var constructorParameter in constructorParameters)
        {
            var optionType = constructorParameter.ParameterType;

            var parser = services.GetService(typeof(IOptionParser<>).MakeGenericType(optionType)) as IOptionParser
                ?? throw new InvalidOperationException($"No option parser registered for type '{optionType.Name}'.");

            var optionValue = (JsonElement?)options.SingleOrDefault(option => option.name == constructorParameter.Name)?.value;

            var parseResult = await parser.ParseAsync(context, optionValue, resolved);
            if (parseResult.Error != null)
            {
                return Error(new ParsingFailed($"⚠️ `{constructorParameter.Name}`: {parseResult.Error.Message}"));
            }

            args.Add(parseResult.Value);
        }

        return Activator.CreateInstance(command.OptionType, [.. args]) ?? throw new InvalidOperationException();
    }

    private List<Button> CreateAndBindButtons(ParsedInteraction interaction, MessageResult m, string authorId)
    {
        if (m.Buttons?.Buttons.Count > 0)
        {
            switch (m.Buttons.Settings)
            {
                case TemporaryButtonSettings temporary:
                    {
                        var buttons = m.Buttons.Buttons
                            .Select(b => b with { Button = b.Button with { Id = $"{Guid.NewGuid():N}-{b.Button.Id}" } })
                            .ToList();

                        RateLimiter followUpLimiter = new ConcurrencyLimiter(
                            new ConcurrencyLimiterOptions { PermitLimit = 1, QueueLimit = 100 });

                        foreach (var button in buttons)
                        {
                            messageComponentHandler.AddCallback(button.Button.Id, async component =>
                            {
                                if (button.AllowNonAuthor || component.Interaction.UserId == authorId)
                                {
                                    try
                                    {
                                        var action = await button.OnClick(component.Interaction.UserId);
                                        switch (action)
                                        {
                                            case UpdateMessage update:
                                                RemoveCallbacks(buttons);
                                                var newButtons = CreateAndBindButtons(interaction, update.NewMessage, authorId);
                                                await CreateInteractionClient().EditOriginalResponseAsync(component.Interaction, message: new(update.NewMessage.Content, newButtons));
                                                break;

                                            case UpdateMessageContent update:
                                                await CreateInteractionClient().EditOriginalResponseAsync(component.Interaction, message: new(update.Content, [.. buttons.Select(b => b.Button)]));
                                                if (update.Response != null)
                                                {
                                                    using var lease = await followUpLimiter.AcquireAsync();
                                                    if (lease.IsAcquired)
                                                    {
                                                        // Throttle to avoid Discord rate limit
                                                        await Task.Delay(TimeSpan.FromMilliseconds(250));

                                                        try
                                                        {
                                                            await CreateInteractionClient().SendFollowupResponseAsync(component.Interaction,
                                                                new(update.Response.Message.Content, IsPrivate: update.Response.IsPrivate));
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            logger.LogError(e, "Unhandled exception trying to send follow up response to button {ButtonId} action:", button.Button.Id);
                                                        }
                                                    }
                                                }
                                                break;

                                            case DeleteMessage:
                                                RemoveCallbacks(buttons);
                                                await CreateInteractionClient().DeleteOriginalResponseAsync(component);
                                                break;

                                            case IgnoreClick:
                                                break;

                                            default: throw new InvalidOperationException();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        logger.LogError(e, "Unhandled exception in button {ButtonId} action:", button.Button.Id);
                                        await CreateInteractionClient().EditOriginalResponseAsync(component.Interaction, message: new(
                                            new(EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that. 😕")),
                                            []
                                        ));
                                    }
                                }
                            });
                        }

                        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(async () =>
                        {
                            await Task.Delay(temporary.ListenToClicksFor ?? TimeSpan.FromMinutes(10));
                            RemoveCallbacks(buttons);
                            await followUpLimiter.DisposeAsync();

                            var result = temporary.OnEnded != null ? await temporary.OnEnded() : null;
                            MessageResponse updated = result != null ? new(result.Content) : new(m.Content, []);

                            await CreateInteractionClient().EditOriginalResponseAsync(interaction, updated);
                        }, nameof(CreateAndBindButtons)));

                        return [.. buttons.Select(b => b.Button)];
                    }

                case PermanentButtonSettings permanent:
                    return [.. m.Buttons.Buttons.Select(b => b.Button)];

                default: throw new NotImplementedException();
            }
        }
        else
        {
            return [];
        }
    }

    private void RemoveCallbacks(List<ButtonResult> buttons)
    {
        foreach (var b in buttons)
            messageComponentHandler.RemoveCallback(b.Button.Id);
    }
}
