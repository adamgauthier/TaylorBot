using Discord;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Program.Events;

namespace TaylorBot.Net.Commands.Events
{
    public interface ISlashCommand
    {
        Type OptionType { get; }
        string Name { get; }
        ValueTask<Command> GetCommandAsync(RunContext context, object options);
    }

    public interface ISlashCommand<T> : ISlashCommand
    {
        Type ISlashCommand.OptionType => typeof(T);
        async ValueTask<Command> ISlashCommand.GetCommandAsync(RunContext context, object options) => await GetCommandAsync(context, (T)options);

        ValueTask<Command> GetCommandAsync(RunContext context, T options);
    }

    public class SlashCommandHandler : IInteractionCreatedHandler
    {
        private readonly ILogger<SlashCommandHandler> _logger;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly ICommandRunner _commandRunner;
        private readonly IOngoingCommandRepository _ongoingCommandRepository;
        private readonly ICommandUsageRepository _commandUsageRepository;
        private readonly IIgnoredUserRepository _ignoredUserRepository;
        private readonly ICommandPrefixRepository _commandPrefixRepository;
        private readonly IReadOnlyDictionary<string, ISlashCommand> _slashCommands;
        private readonly IReadOnlyDictionary<Type, IOptionParser> _optionParsers;
        private readonly HttpClient _httpClient = new();

        public SlashCommandHandler(
            ILogger<SlashCommandHandler> logger,
            ITaylorBotClient taylorBotClient,
            ICommandRunner commandRunner,
            IOngoingCommandRepository ongoingCommandRepository,
            ICommandUsageRepository commandUsageRepository,
            IIgnoredUserRepository ignoredUserRepository,
            ICommandPrefixRepository commandPrefixRepository,
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
            _slashCommands = services.GetServices<ISlashCommand>().ToDictionary(c => c.Name);
            _optionParsers = services.GetServices<IOptionParser>().ToDictionary(c => c.OptionType);
        }

        private const byte ApplicationCommandInteractionType = 2;
        private const byte SubCommandOptionType = 1;
        private const byte SubCommandGroupOptionType = 2;


        private const byte ChannelMessageWithSourceInteractionResponseType = 4;
        private const byte EphemeralInteractionResponseFlags = 64;

        private record InteractionResponse(byte type, InteractionResponse.InteractionApplicationCommandCallbackData data)
        {
            public record InteractionApplicationCommandCallbackData(string? content = null, IReadOnlyList<Embed>? embeds = null, byte? flags = null);

            public record Embed(string? title, string? description, EmbedAuthor? author, EmbedImage? image, uint? color);

            public record EmbedAuthor(string? name, string? url, string? icon_url);

            public record EmbedImage(string? url);
        }

        private record ApplicationCommand(
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

        public async Task InteractionCreatedAsync(Interaction interaction)
        {
            if (interaction.type == ApplicationCommandInteractionType)
            {
                try
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
                catch (Exception e)
                {
                    _logger.LogError(e, "Unhandled exception in ApplicationCommand:");
                }
            }
        }

        private async ValueTask HandleApplicationCommand(ApplicationCommand interaction)
        {
            var channel = (IMessageChannel)await _taylorBotClient.ResolveRequiredChannelAsync(new(interaction.ChannelId));

            var author = channel is ITextChannel text ?
                (await _taylorBotClient.ResolveGuildUserAsync(
                    text.Guild,
                    new(interaction.Guild!.Member.user.id)
                ))! :
                await _taylorBotClient.ResolveRequiredUserAsync(new(interaction.UserData!.id));

            var oldPrefix = channel is ITextChannel textChannel ?
                await _commandPrefixRepository.GetOrInsertGuildPrefixAsync(textChannel.Guild) :
                string.Empty;

            var context = new RunContext(
                DateTimeOffset.Now,
                author,
                channel,
                author is IGuildUser guildUser ? guildUser.Guild : null,
                _taylorBotClient.DiscordShardedClient,
                oldPrefix,
                new()
            );

            var (commandName, options) = GetFullCommandNameAndOptions(interaction.Data);

            if (_slashCommands.TryGetValue(commandName, out var slashCommand))
            {
                _logger.LogInformation($"{context.User.FormatLog()} using slash command '{slashCommand.Name}' ({interaction.Data.id}) in {context.Channel.FormatLog()}");

                var result = await RunCommandAsync(slashCommand, context, options);

                if (context.OnGoingState.OnGoingCommandAddedToPool != null)
                {
                    await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, context.OnGoingState.OnGoingCommandAddedToPool);
                }

                switch (result)
                {
                    case EmbedResult embedResult:
                        var embed = embedResult.Embed;

                        var response = new InteractionResponse(
                            ChannelMessageWithSourceInteractionResponseType,
                            new(string.Empty, new[] {
                                ToInteractionEmbed(embed)
                            })
                        );

                        await SendResponseAsync(interaction, response);
                        break;

                    case ParsingFailed parsingFailed:
                        await SendResponseAsync(interaction, new InteractionResponse(
                            ChannelMessageWithSourceInteractionResponseType,
                            new(parsingFailed.Message, flags: EphemeralInteractionResponseFlags)
                        ));
                        break;

                    case PreconditionFailed preconditionFailed:
                        _logger.LogInformation($"{context.User.FormatLog()} precondition failure: {preconditionFailed.PrivateReason}.");
                        await SendResponseAsync(interaction, new InteractionResponse(
                            ChannelMessageWithSourceInteractionResponseType,
                            new(string.Empty, new[] {
                                ToInteractionEmbed(new EmbedBuilder()
                                    .WithColor(TaylorBotColors.ErrorColor)
                                    .WithDescription(preconditionFailed.UserReason.Reason)
                                .Build())
                            })
                        ));
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

                        await SendResponseAsync(interaction, new InteractionResponse(
                            ChannelMessageWithSourceInteractionResponseType,
                            new(string.Empty, new[] {
                                ToInteractionEmbed(new EmbedBuilder()
                                    .WithColor(TaylorBotColors.ErrorColor)
                                    .WithDescription(string.Join('\n', baseDescriptionLines))
                                .Build())
                            })
                        ));
                        break;

                    default:
                        throw new InvalidOperationException($"Unexpected command result: {result.GetType()}");
                }
            }
        }

        private static (string, IReadOnlyList<Interaction.ApplicationCommandInteractionDataOption>? options) GetFullCommandNameAndOptions(Interaction.ApplicationCommandInteractionData data)
        {
            if (data.options != null && data.options.Count == 1 && data.options[0].type == SubCommandGroupOptionType)
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
                if (parsedOptions is ParsingFailed failed)
                    return failed;

                var command = await slashCommand.GetCommandAsync(context, parsedOptions);

                var result = await _commandRunner.RunAsync(command, context);

                _commandUsageRepository.QueueIncrementSuccessfulUseCount(slashCommand.Name);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unhandled exception in slash command '{slashCommand.Name}':");
                _commandUsageRepository.QueueIncrementUnhandledErrorCount(slashCommand.Name);
                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.ErrorColor)
                    .WithDescription($"Oops, an unknown command error occurred. Sorry about that. 😕")
                .Build());
            }
        }

        private async ValueTask<object> ParseOptionsAsync(ISlashCommand command, RunContext context, IReadOnlyList<Interaction.ApplicationCommandInteractionDataOption>? options)
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
                var parser = _optionParsers[constructorParameter.ParameterType];

                var parseResult = await parser.ParseAsync(context, (JsonElement?)options.SingleOrDefault(option => option.name == constructorParameter.Name)?.value);

                if (parseResult is ParsingFailed failed)
                    return new ParsingFailed($"⚠ `{constructorParameter.Name}`: {failed.Message}");

                args.Add(parseResult);
            }

            return Activator.CreateInstance(command.OptionType, args.ToArray()) ?? throw new InvalidOperationException();
        }

        private InteractionResponse.Embed ToInteractionEmbed(Embed embed)
        {
            return new InteractionResponse.Embed(
                title: embed.Title,
                description: embed.Description,
                author: embed.Author.HasValue ? new(embed.Author.Value.Name, embed.Author.Value.Url, embed.Author.Value.IconUrl) : null,
                image: embed.Image.HasValue ? new(embed.Image.Value.Url) : null,
                color: embed.Color.HasValue ? embed.Color.Value.RawValue : null
            );
        }

        private async ValueTask SendResponseAsync(ApplicationCommand interaction, InteractionResponse interactionResponse)
        {
            var response = await _httpClient.PostAsync(
                $"https://discord.com/api/v8/interactions/{interaction.Id}/{interaction.Token}/callback",
                JsonContent.Create(interactionResponse)
            );

            response.EnsureSuccessStatusCode();
        }
    }
}
