using Discord;
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
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.PostExecution
{
    public record SlashCommandInfo(string Name, bool IsPrivateResponse = false);

    public interface ISlashCommand
    {
        Type OptionType { get; }
        ValueTask<Command> GetCommandAsync(RunContext context, object options);

        SlashCommandInfo Info { get; }
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
                await _interactionResponseClient.SendAcknowledgementResponseAsync(interaction, IsEphemeral: slashCommand.Info.IsPrivateResponse);

                var author = interaction.Guild != null ?
                    (await _taylorBotClient.Value.ResolveGuildUserAsync(
                        new SnowflakeId(interaction.Guild.Id),
                        new(interaction.Guild.Member.user.id)
                    ))! :
                    await _taylorBotClient.Value.ResolveRequiredUserAsync(new(interaction.UserData!.id));

                var oldPrefix = author is IGuildUser aGuildUser ?
                    await _commandPrefixRepository.GetOrInsertGuildPrefixAsync(aGuildUser.Guild) :
                    string.Empty;

                var context = new RunContext(
                    DateTimeOffset.Now,
                    author,
                    new(interaction.ChannelId),
                    author is IGuildUser guildUser ? guildUser.Guild : null,
                    _taylorBotClient.Value.DiscordShardedClient,
                    oldPrefix,
                    new()
                );

                _logger.LogInformation(
                    $"{context.User.FormatLog()} using slash command '{slashCommand.Info.Name}' ({interaction.Data.id}) in channel {context.Channel.Id}{(context.Guild != null ? $" on {context.Guild.FormatLog()}" : "")}"
                );

                var result = await RunCommandAsync(slashCommand, context, options);

                if (context.OnGoingState.OnGoingCommandAddedToPool != null)
                {
                    await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, context.OnGoingState.OnGoingCommandAddedToPool);
                }

                switch (result)
                {
                    case EmbedResult embedResult:
                        await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(embedResult.Embed));
                        break;

                    case MessageResult messageResult:
                        IReadOnlyList<Button> CreateAndBindButtons(MessageResult m, string authorId)
                        {
                            var buttons = m.Buttons != null ?
                                m.Buttons.Select(b => b with { Button = b.Button with { Id = $"{Guid.NewGuid():N}-{b.Button.Id}" } }).ToList() :
                                new List<MessageResult.ButtonResult>();

                            foreach (var button in buttons)
                            {
                                _messageComponentHandler.AddCallback(button.Button.Id, async component =>
                                {
                                    if (component.UserId == authorId)
                                    {
                                        _messageComponentHandler.RemoveCallback(button.Button.Id);

                                        var updated = await button.Action();
                                        if (updated != null)
                                        {
                                            var buttons = CreateAndBindButtons(updated, authorId);
                                            await _interactionResponseClient.EditOriginalResponseAsync(component, new(updated.Content, buttons));
                                        }
                                        else
                                        {
                                            await _interactionResponseClient.DeleteOriginalResponseAsync(component);
                                        }
                                    }
                                }, expire: TimeSpan.FromMinutes(10));
                            }

                            return buttons.Select(b => b.Button).ToList();
                        }

                        var buttons = CreateAndBindButtons(messageResult, author.Id.ToString());

                        await _interactionResponseClient.SendFollowupResponseAsync(
                            interaction,
                            new(messageResult.Content, buttons)
                        );
                        break;

                    case ParsingFailed parsingFailed:
                        await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(EmbedFactory.CreateError(parsingFailed.Message)));
                        break;

                    case PreconditionFailed preconditionFailed:
                        _logger.LogInformation($"{context.User.FormatLog()} precondition failure: {preconditionFailed.PrivateReason}.");
                        await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(EmbedFactory.CreateError(preconditionFailed.UserReason.Reason)));
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

                        await _interactionResponseClient.SendFollowupResponseAsync(interaction, new(EmbedFactory.CreateError(string.Join('\n', baseDescriptionLines))));
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected command result: {result.GetType()}");
                }
            }
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
                _logger.LogError(e, $"Unhandled exception in slash command '{slashCommand.Info.Name}':");
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
    }
}
