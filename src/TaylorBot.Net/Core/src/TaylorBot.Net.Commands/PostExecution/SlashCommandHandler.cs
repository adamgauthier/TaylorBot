using Discord;
using FakeItEasy;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OperationResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.PostExecution
{
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

    public interface ISlashCommand<T> : ISlashCommand
    {
        Type ISlashCommand.OptionType => typeof(T);
        async ValueTask<Command> ISlashCommand.GetCommandAsync(RunContext context, object options) => await GetCommandAsync(context, (T)options);

        ValueTask<Command> GetCommandAsync(RunContext context, T options);
    }

    public record ApplicationCommand(
        string Id,
        string Token,
        Interaction.ApplicationCommandInteractionData Data,
        ApplicationCommand.GuildData? Guild,
        Interaction.User? UserData,
        string ChannelId
    )
    {
        public record GuildData(string Id, Interaction.GuildMember Member);
    }

    public class SlashCommandHandler
    {
        private readonly ILogger<SlashCommandHandler> _logger;
        private readonly Lazy<ITaylorBotClient> _taylorBotClient;
        private readonly ICommandRunner _commandRunner;
        private readonly IOngoingCommandRepository _ongoingCommandRepository;
        private readonly ICommandUsageRepository _commandUsageRepository;
        private readonly IIgnoredUserRepository _ignoredUserRepository;
        private readonly ICommandPrefixRepository _commandPrefixRepository;
        private readonly Lazy<IReadOnlyDictionary<string, ISlashCommand>> _slashCommands;
        private readonly Lazy<IReadOnlyDictionary<Type, IOptionParser>> _optionParsers;
        private readonly InteractionResponseClient _interactionResponseClient;
        private readonly MessageComponentHandler _messageComponentHandler;
        private readonly ModalInteractionHandler _modalInteractionHandler;
        private readonly TaskExceptionLogger _taskExceptionLogger;

        public SlashCommandHandler(
            ILogger<SlashCommandHandler> logger,
            Lazy<ITaylorBotClient> taylorBotClient,
            ICommandRunner commandRunner,
            IOngoingCommandRepository ongoingCommandRepository,
            ICommandUsageRepository commandUsageRepository,
            IIgnoredUserRepository ignoredUserRepository,
            ICommandPrefixRepository commandPrefixRepository,
            InteractionResponseClient interactionResponseClient,
            MessageComponentHandler messageComponentHandler,
            ModalInteractionHandler modalInteractionHandler,
            TaskExceptionLogger taskExceptionLogger,
            IServiceProvider services
        )
        {
            _logger = logger;
            _taylorBotClient = taylorBotClient;
            _commandRunner = commandRunner;
            _ongoingCommandRepository = ongoingCommandRepository;
            _commandUsageRepository = commandUsageRepository;
            _ignoredUserRepository = ignoredUserRepository;
            _commandPrefixRepository = commandPrefixRepository;
            _interactionResponseClient = interactionResponseClient;
            _messageComponentHandler = messageComponentHandler;
            _modalInteractionHandler = modalInteractionHandler;
            _taskExceptionLogger = taskExceptionLogger;
            _slashCommands = new(() => services.GetServices<ISlashCommand>().ToDictionary(c => c.Info.Name));
            _optionParsers = new(() => services.GetServices<IOptionParser>().ToDictionary(c => c.OptionType));
        }

        public async ValueTask HandleAsync(Interaction interaction)
        {
            await HandleApplicationCommand(new(
                interaction.id,
                interaction.token,
                interaction.data!,
                interaction.member != null ? new(interaction.guild_id!, interaction.member) : null,
                interaction.user,
                interaction.channel_id!
            ));
        }

        private async ValueTask HandleApplicationCommand(ApplicationCommand interaction)
        {
            var (commandName, options) = GetFullCommandNameAndOptions(interaction.Data);

            if (_slashCommands.Value.TryGetValue(commandName, out var slashCommand))
            {
                RunContext context = await CreateRunContextAsync(interaction, slashCommand);

                var result = await RunCommandAsync(slashCommand, context, options);

                if (context.OnGoing.OnGoingCommandAddedToPool != null)
                {
                    await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, context.OnGoing.OnGoingCommandAddedToPool);
                }

                switch (result)
                {
                    case EmbedResult embedResult:
                        if (context.WasAcknowledged)
                        {
                            await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(embedResult.Embed));
                        }
                        else
                        {
                            await _interactionResponseClient.SendImmediateResponseAsync(interaction, new(embedResult.Embed));
                        }
                        break;

                    case MessageResult messageResult:
                        IReadOnlyList<Button>? buttons = CreateAndBindButtons(interaction, messageResult, context.User.Id.ToString());

                        await _interactionResponseClient.SendFollowupResponseAsync(
                            interaction,
                            new(messageResult.Content, buttons)
                        );
                        break;

                    case CreateModalResult createModal:
                        var submitted = false;

                        var modal = createModal with { Id = $"{Guid.NewGuid():N}-{createModal.Id}" };

                        _modalInteractionHandler.AddCallback(modal.Id, new(async submit =>
                        {
                            if (submit.UserId == $"{context.User.Id}")
                            {
                                _modalInteractionHandler.RemoveCallback(submit.CustomId);
                                submitted = true;

                                try
                                {
                                    var result = await modal.SubmitAction(submit);
                                    var buttons = CreateAndBindButtons(interaction, result, $"{context.User.Id}");
                                    await _interactionResponseClient.EditOriginalResponseAsync(submit, new(result.Content, buttons, IsPrivate: createModal.IsPrivateResponse));
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Unhandled exception in modal submit {Id} action:", submit.Id);
                                    await _interactionResponseClient.EditOriginalResponseAsync(submit, new(
                                        EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that. 😕")
                                    ));
                                }
                            }
                        }, createModal.IsPrivateResponse));

                        _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMinutes(10));

                            if (!submitted)
                            {
                                _modalInteractionHandler.RemoveCallback(modal.Id);
                            }
                        }, nameof(CreateModalResult)));

                        await _interactionResponseClient.SendModalResponseAsync(interaction, modal);
                        break;

                    case ParsingFailed parsingFailed:
                        if (context.WasAcknowledged)
                        {
                            await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(EmbedFactory.CreateError(parsingFailed.Message)));
                        }
                        else
                        {
                            await _interactionResponseClient.SendImmediateResponseAsync(interaction, new(new(EmbedFactory.CreateError(parsingFailed.Message)), IsPrivate: true));
                        }
                        break;

                    case PreconditionFailed preconditionFailed:
                        _logger.LogInformation("{User} precondition failure: {PrivateReason}.", context.User.FormatLog(), preconditionFailed.PrivateReason);
                        if (context.WasAcknowledged)
                        {
                            await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(EmbedFactory.CreateError(preconditionFailed.UserReason.Reason)));
                        }
                        else
                        {
                            await _interactionResponseClient.SendImmediateResponseAsync(interaction, new(new(EmbedFactory.CreateError(preconditionFailed.UserReason.Reason)), IsPrivate: true));
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
                            baseDescriptionLines = baseDescriptionLines
                                .Append("**Stop trying to perform this action or all your commands will be ignored.**")
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

                        await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(EmbedFactory.CreateError(string.Join('\n', baseDescriptionLines))));
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected command result: {result.GetType()}");
                }
            }
        }

        private async Task<RunContext> CreateRunContextAsync(ApplicationCommand interaction, ISlashCommand slashCommand)
        {
            RunContext context;

            switch (slashCommand.Info)
            {
                case MessageCommandInfo info:
                    await _interactionResponseClient.SendAckResponseWithLoadingMessageAsync(interaction, IsEphemeral: info.IsPrivateResponse);

                    var author = interaction.Guild != null ?
                        (await _taylorBotClient.Value.ResolveGuildUserAsync(
                            new SnowflakeId(interaction.Guild.Id),
                            new(interaction.Guild.Member.user.id)
                        ))! :
                        await _taylorBotClient.Value.ResolveRequiredUserAsync(new(interaction.UserData!.id));

                    var oldPrefix = author is IGuildUser aGuildUser ?
                        await _commandPrefixRepository.GetOrInsertGuildPrefixAsync(aGuildUser.Guild) :
                        string.Empty;

                    context = new RunContext(
                        DateTimeOffset.Now,
                        author,
                        new(interaction.ChannelId),
                        author is IGuildUser authorGuildUser ? authorGuildUser.Guild : null,
                        _taylorBotClient.Value.DiscordShardedClient,
                        _taylorBotClient.Value.DiscordShardedClient.CurrentUser,
                        new(interaction.Data.id, interaction.Data.name),
                        oldPrefix,
                        new()
                    );

                    _logger.LogInformation(
                        "{User} using slash command '{CommandName}' ({InteractionId}) in channel {ChannelId}{GuildInfo}",
                        context.User.FormatLog(), slashCommand.Info.Name, interaction.Data.id, context.Channel.Id, context.Guild != null ? $" on {context.Guild.FormatLog()}" : ""
                    );
                    break;

                case ModalCommandInfo info:
                    IUser user;
                    if (interaction.Guild != null)
                    {
                        var guild = A.Fake<IGuild>(o => o.Strict());
                        A.CallTo(() => guild.Id).Returns(new SnowflakeId(interaction.Guild.Id).Id);
                        // Assuming GuildPermissions will cover use cases where this property is used
                        A.CallTo(() => guild.OwnerId).Returns(ulong.MaxValue);

                        var fakeGuildUser = A.Fake<IGuildUser>(o => o.Strict());
                        A.CallTo(() => fakeGuildUser.Id).Returns(new SnowflakeId(interaction.Guild.Member.user.id).Id);
                        A.CallTo(() => fakeGuildUser.Guild).Returns(guild);
                        A.CallTo(() => fakeGuildUser.GuildId).Returns(guild.Id);
                        A.CallTo(() => fakeGuildUser.Username).Returns(interaction.Guild.Member.user.username);
                        A.CallTo(() => fakeGuildUser.Discriminator).Returns(interaction.Guild.Member.user.discriminator);
                        A.CallTo(() => fakeGuildUser.GuildPermissions).Returns(new GuildPermissions(interaction.Guild.Member.permissions));
                        A.CallTo(() => fakeGuildUser.JoinedAt).Returns(DateTimeOffset.Parse(interaction.Guild.Member.joined_at));
                        A.CallTo(() => fakeGuildUser.Mention).Returns(MentionUtils.MentionUser(fakeGuildUser.Id));
                        user = fakeGuildUser;
                    }
                    else
                    {
                        user = A.Fake<IUser>(o => o.Strict());
                        A.CallTo(() => user.Id).Returns(new SnowflakeId(interaction.UserData!.id).Id);
                        A.CallTo(() => user.Username).Returns(interaction.UserData!.username);
                        A.CallTo(() => user.Discriminator).Returns(interaction.UserData!.discriminator);
                        A.CallTo(() => user.Mention).Returns(MentionUtils.MentionUser(user.Id));
                    }

                    context = new RunContext(
                        DateTimeOffset.Now,
                        user,
                        new(interaction.ChannelId),
                        user is IGuildUser guildUser ? guildUser.Guild : null,
                        _taylorBotClient.Value.DiscordShardedClient,
                        _taylorBotClient.Value.DiscordShardedClient.CurrentUser,
                        new(interaction.Data.id, interaction.Data.name),
                        string.Empty,
                        new(),
                        WasAcknowledged: false
                    );

                    _logger.LogInformation(
                        "{User} using modal command '{CommandName}' ({InteractionId}) in channel {ChannelId}{GuildInfo}",
                        context.User.FormatLog(), slashCommand.Info.Name, interaction.Data.id, context.Channel.Id, context.Guild != null ? $" on {context.Guild.Id}" : ""
                    );
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected SlashCommandInfo: {slashCommand.Info.GetType()}");
            }

            return context;
        }

        private const byte SubCommandOptionType = 1;
        private const byte SubCommandGroupOptionType = 2;

        private static (string, IReadOnlyList<Interaction.ApplicationCommandInteractionDataOption>? options) GetFullCommandNameAndOptions(Interaction.ApplicationCommandInteractionData data)
        {
            if (data.options != null && data.options.Count == 1)
            {
                if (data.options[0].type == SubCommandOptionType)
                {
                    return ($"{data.name} {data.options[0].name}", data.options[0].options);
                }
                else if (data.options[0].type == SubCommandGroupOptionType)
                {
                    var subOptions = data.options[0].options;
                    if (subOptions != null && subOptions.Count == 1 && subOptions[0].type == SubCommandOptionType)
                    {
                        return ($"{data.name} {data.options[0].name} {subOptions[0].name}", subOptions[0].options);
                    }
                    else
                    {
                        throw new ArgumentException("Expected sub command group's only option to be a sub command.");
                    }
                }
            }

            return (data.name, data.options);
        }

        private async ValueTask<ICommandResult> RunCommandAsync(ISlashCommand slashCommand, RunContext context, IReadOnlyList<Interaction.ApplicationCommandInteractionDataOption>? options)
        {
            try
            {
                var parsedOptions = await ParseOptionsAsync(slashCommand, context, options);
                if (parsedOptions.Error != null)
                    return parsedOptions.Error;

                var command = await slashCommand.GetCommandAsync(context, parsedOptions.Value);

                var result = await _commandRunner.RunAsync(command, context);

                _commandUsageRepository.QueueIncrementSuccessfulUseCount(slashCommand.Info.Name);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception in slash command '{CommandName}':", slashCommand.Info.Name);
                _commandUsageRepository.QueueIncrementUnhandledErrorCount(slashCommand.Info.Name);
                return new EmbedResult(EmbedFactory.CreateError($"Oops, an unknown command error occurred. Sorry about that. 😕"));
            }
        }

        private async ValueTask<Result<object, ParsingFailed>> ParseOptionsAsync(ISlashCommand command, RunContext context, IReadOnlyList<Interaction.ApplicationCommandInteractionDataOption>? options)
        {
            if (command.OptionType == typeof(NoOptions))
                return new NoOptions();

            if (options == null)
                options = Array.Empty<Interaction.ApplicationCommandInteractionDataOption>();

            var constructorParameters = command.OptionType.GetConstructors().Single().GetParameters();

            var optionWithoutMatch = options.FirstOrDefault(o => !constructorParameters.Any(p => p.Name == o.name));

            if (optionWithoutMatch != null)
                throw new InvalidOperationException($"Found no parameter mapping in '{command.OptionType}' for option '{optionWithoutMatch.name}'.");

            List<object?> args = new();

            foreach (var constructorParameter in constructorParameters)
            {
                var parser = _optionParsers.Value[constructorParameter.ParameterType];

                var parseResult = await parser.ParseAsync(context, (JsonElement?)options.SingleOrDefault(option => option.name == constructorParameter.Name)?.value);

                if (parseResult.Error != null)
                    return Error(new ParsingFailed($"⚠ `{constructorParameter.Name}`: {parseResult.Error.Message}"));

                args.Add(parseResult.Value);
            }

            return Activator.CreateInstance(command.OptionType, args.ToArray()) ?? throw new InvalidOperationException();
        }

        private IReadOnlyList<Button> CreateAndBindButtons(ApplicationCommand interaction, MessageResult m, string authorId)
        {
            var buttons = m.Buttons != null ?
                m.Buttons.Select(b => b with { Button = b.Button with { Id = $"{Guid.NewGuid():N}-{b.Button.Id}" } }).ToList() :
                new List<MessageResult.ButtonResult>();

            var buttonPressed = false;

            foreach (var button in buttons)
            {
                _messageComponentHandler.AddCallback(button.Button.Id, async component =>
                {
                    if (component.UserId == authorId)
                    {
                        buttonPressed = true;

                        foreach (var b in buttons)
                            _messageComponentHandler.RemoveCallback(b.Button.Id);

                        try
                        {
                            var updated = await button.Action();
                            if (updated != null)
                            {
                                var buttons = CreateAndBindButtons(interaction, updated, authorId);
                                await _interactionResponseClient.EditOriginalResponseAsync(component, new(updated.Content, buttons));
                            }
                            else
                            {
                                await _interactionResponseClient.DeleteOriginalResponseAsync(component);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Unhandled exception in button {button.Button.Id} action:");
                            await _interactionResponseClient.EditOriginalResponseAsync(component, new(
                                new(EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that. 😕")),
                                Array.Empty<Button>()
                            ));
                        }
                    }
                });
            }

            _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(10));

                if (!buttonPressed)
                {
                    foreach (var b in buttons)
                        _messageComponentHandler.RemoveCallback(b.Button.Id);

                    await _interactionResponseClient.EditOriginalResponseAsync(interaction, new(m.Content, Array.Empty<Button>()));
                }
            }, nameof(CreateAndBindButtons)));

            return buttons.Select(b => b.Button).ToList();
        }
    }
}
