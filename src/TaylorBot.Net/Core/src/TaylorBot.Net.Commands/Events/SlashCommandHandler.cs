using Discord;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Program.Events;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Events
{
    public interface ISlashCommand
    {
        string Name { get; }
        ValueTask<Command> GetCommandAsync(RunContext context, Interaction.ApplicationCommandInteractionData data);
    }

    public class SlashCommandHandler : IInteractionCreatedHandler
    {
        private readonly ILogger<SlashCommandHandler> _logger;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly ICommandRunner _commandRunner;
        private readonly IOngoingCommandRepository _ongoingCommandRepository;
        private readonly ICommandUsageRepository _commandUsageRepository;
        private readonly IIgnoredUserRepository _ignoredUserRepository;
        private readonly IReadOnlyDictionary<string, ISlashCommand> _slashCommands;
        private readonly ICommandPrefixRepository _commandPrefixRepository;
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
        }

        private const byte ApplicationCommandInteractionType = 2;
        private const byte ChannelMessageWithSourceInteractionResponseType = 4;

        private record InteractionResponse(byte type, InteractionResponse.InteractionApplicationCommandCallbackData data)
        {
            public record InteractionApplicationCommandCallbackData(IReadOnlyList<Embed> embeds);

            public record Embed(string? description, EmbedAuthor? author, EmbedImage? image, uint? color);

            public record EmbedAuthor(string? name, string? url, string? icon_url);

            public record EmbedImage(string? url);
        }

        public async Task InteractionCreatedAsync(Interaction interaction)
        {
            if (interaction.type == ApplicationCommandInteractionType)
            {
                try
                {
                    await HandleApplicationCommand(interaction);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unhandled exception in ApplicationCommand:");
                }
            }
        }

        private async ValueTask HandleApplicationCommand(Interaction interaction)
        {
            var author = interaction.user?.id != null ?
                await _taylorBotClient.ResolveRequiredUserAsync(new(interaction.user!.id)) :
                (await _taylorBotClient.ResolveGuildUserAsync(_taylorBotClient.DiscordShardedClient.GetGuild(new SnowflakeId(interaction.guild_id!).Id), new(interaction.member!.user.id)))!;

            var channel = (IMessageChannel)await _taylorBotClient.ResolveRequiredChannelAsync(new(interaction.channel_id!));

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

            if (_slashCommands.TryGetValue(interaction.data!.name, out var slashCommand))
            {
                _logger.LogInformation($"{context.User.FormatLog()} using slash command '{slashCommand.Name}' ({interaction.data!.id}) in {context.Channel.FormatLog()}");

                var result = await RunCommandAsync(slashCommand, context, interaction);

                switch (result)
                {
                    case EmbedResult embedResult:
                        var embed = embedResult.Embed;

                        var response = new InteractionResponse(
                            ChannelMessageWithSourceInteractionResponseType,
                            new(new[] {
                                ToInteractionEmbed(embed)
                            })
                        );

                        await SendResponseAsync(interaction, response);
                        break;

                    case PreconditionFailed failed:
                        _logger.LogInformation($"{context.User.FormatLog()} precondition failure: {failed.PrivateReason}.");
                        if (failed.UserReason != null)
                        {
                            await SendResponseAsync(interaction, new InteractionResponse(
                                ChannelMessageWithSourceInteractionResponseType,
                                new(new[] {
                                    ToInteractionEmbed(new EmbedBuilder()
                                        .WithColor(TaylorBotColors.ErrorColor)
                                        .WithDescription($"{context.User.Mention} {failed.UserReason}")
                                    .Build())
                                })
                            ));
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
                            new(new[] {
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

                if (context.OnGoingState.OnGoingCommandAddedToPool != null)
                {
                    await _ongoingCommandRepository.RemoveOngoingCommandAsync(context.User, context.OnGoingState.OnGoingCommandAddedToPool);
                }
            }
        }

        private async ValueTask<ICommandResult> RunCommandAsync(ISlashCommand slashCommand, RunContext context, Interaction interaction)
        {
            try
            {
                var command = await slashCommand.GetCommandAsync(context, interaction.data!);
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

        private InteractionResponse.Embed ToInteractionEmbed(Embed embed)
        {
            return new InteractionResponse.Embed(
                description: embed.Description,
                author: embed.Author.HasValue ? new(embed.Author.Value.Name, embed.Author.Value.Url, embed.Author.Value.IconUrl) : null,
                image: embed.Image.HasValue ? new(embed.Image.Value.Url) : null,
                color: embed.Color.HasValue ? embed.Color.Value.RawValue : null
            );
        }

        private async ValueTask SendResponseAsync(Interaction interaction, InteractionResponse response)
        {
            await _httpClient.PostAsync(
                $"https://discord.com/api/v8/interactions/{interaction.id}/{interaction.token}/callback",
                JsonContent.Create(response)
            );
        }
    }
}
