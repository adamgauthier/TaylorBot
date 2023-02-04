using Discord;
using FakeItEasy;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.RandomGeneration.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Domain
{
    public record Giveaway(List<SnowflakeId> Entrants, DateTimeOffset EndsAt, int TaypointPrize)
    {
        public IUserMessage? OriginalMessage { get; set; } = null;
    }

    public class ValentineGiveawayDomainService
    {
        private readonly ILogger<ValentineGiveawayDomainService> _logger;
        private readonly IValentinesRepository _valentinesRepository;
        private readonly ITaypointRewardRepository _taypointRepository;
        private readonly ITaylorBotClient _taylorBotClient;
        private readonly MessageComponentHandler _messageComponentHandler;
        private readonly InteractionResponseClient _interactionResponseClient;
        private readonly ICryptoSecureRandom _cryptoSecureRandom;

        private Giveaway? _giveaway = null;

        public ValentineGiveawayDomainService(
            ILogger<ValentineGiveawayDomainService> logger,
            IValentinesRepository valentinesRepository,
            ITaypointRewardRepository taypointRepository,
            ITaylorBotClient taylorBotClient,
            MessageComponentHandler messageComponentHandler,
            InteractionResponseClient interactionResponseClient,
            ICryptoSecureRandom cryptoSecureRandom
        )
        {
            _logger = logger;
            _valentinesRepository = valentinesRepository;
            _taypointRepository = taypointRepository;
            _taylorBotClient = taylorBotClient;
            _messageComponentHandler = messageComponentHandler;
            _interactionResponseClient = interactionResponseClient;
            _cryptoSecureRandom = cryptoSecureRandom;
        }

        private record PreviousGiveaway(ulong OriginalMessageId)
        {
            public RewardedUserResult? Winner { get; set; }
            public int AmountWon { get; set; }
        }

        public async Task StartGiveawayAsync()
        {
            while (true)
            {
                var nextRun = TimeSpan.FromMinutes(5);
                try
                {
                    var config = await _valentinesRepository.GetConfigurationAsync();
                    if (DateTimeOffset.UtcNow < config.GiveawaysEndTime)
                    {
                        nextRun = config.TimeSpanBetweenGiveaways;
                        var lounge = (ITextChannel)await _taylorBotClient.ResolveRequiredChannelAsync(config.LoungeChannelId);
                        var id = "enter-giveaway";

                        PreviousGiveaway? previousGiveaway = null;

                        if (_giveaway != null)
                        {
                            _messageComponentHandler.RemoveCallback(id);

                            if (_giveaway.OriginalMessage != null)
                            {
                                previousGiveaway = new(_giveaway.OriginalMessage.Id);

                                var entrants = _giveaway.Entrants.ToList();
                                if (entrants.Any())
                                {
                                    var winner = _cryptoSecureRandom.GetRandomElement(_giveaway.Entrants);

                                    var user = A.Fake<IUser>(o => o.Strict());
                                    A.CallTo(() => user.Id).Returns(winner.Id);
                                    var rewarded = (await _taypointRepository.RewardUsersAsync(new[] { user }, _giveaway.TaypointPrize)).Single();

                                    await _giveaway.OriginalMessage.ModifyAsync(m =>
                                    {
                                        m.Embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                                            $"This {entrants.Count} entrant giveaway ended!",
                                            $"Congratulations to {MentionUtils.MentionUser(rewarded.UserId.Id)} for winning {"taypoint".ToQuantity(_giveaway.TaypointPrize, TaylorBotFormats.BoldReadable)}! 🥳💖",
                                        }));
                                        m.Components = new ComponentBuilder().Build();
                                    });

                                    previousGiveaway.Winner = rewarded;
                                    previousGiveaway.AmountWon = _giveaway.TaypointPrize;
                                }
                                else
                                {
                                    await _giveaway.OriginalMessage.ModifyAsync(m =>
                                    {
                                        m.Embed = EmbedFactory.CreateError("Giveaway ended, but there were no entrants. 😔");
                                        m.Components = new ComponentBuilder().Build();
                                    });
                                }
                            }

                            _giveaway = null;
                        }

                        var builder = new ComponentBuilder()
                            .WithButton(label: "Enter giveaway", customId: id, emote: new Emoji("🎉"));

                        _messageComponentHandler.AddCallback(id, async component =>
                        {
                            try
                            {
                                if (_giveaway != null && !_giveaway.Entrants.Contains(new(component.UserId)))
                                {
                                    var user = A.Fake<IGuildUser>(o => o.Strict());
                                    A.CallTo(() => user.Id).Returns(new SnowflakeId(component.UserId).Id);
                                    var given = await _valentinesRepository.GetRoleObtainedFromUserAsync(user);

                                    if (given.Count >= config.SpreadLimit)
                                    {
                                        _giveaway.Entrants.Add(new(component.UserId));

                                        await _interactionResponseClient.EditOriginalResponseAsync(component, new(
                                            new(new[] { BuildGiveawayEmbed() }, Content: _giveaway?.OriginalMessage?.Content ?? ""),
                                            new[] { new Button("enter-giveaway", ButtonStyle.Primary, "Enter giveaway", "🎉") }
                                        ));

                                        await _interactionResponseClient.SendFollowupResponseAsync(component,
                                            new(new(EmbedFactory.CreateSuccess("You are now entered into this giveaway! 🗳️")), IsEphemeral: true));
                                    }
                                    else
                                    {
                                        await _interactionResponseClient.SendFollowupResponseAsync(component,
                                            new(new(EmbedFactory.CreateError(string.Join('\n', new[] {
                                                $"You can't enter a giveaway because you haven't spread love to **{config.SpreadLimit}** people yet ({given.Count}/{config.SpreadLimit})! ⛔",
                                                $"Please use /love spread to give the role to someone who doesn't have it yet. 😊"
                                            }))), IsEphemeral: true));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Unhandled exception in button {id}:");
                                await _interactionResponseClient.SendFollowupResponseAsync(component,
                                    new(new(EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that. 😕")), IsEphemeral: true));
                            }
                        });

                        var getWinnerMessage = (RewardedUserResult r, int won) =>
                            $"{MentionUtils.MentionUser(r.UserId.Id)} You won {"taypoint".ToQuantity(won, TaylorBotFormats.BoldReadable)} from the previous giveaway! You now have {r.NewTaypointCount.ToString(TaylorBotFormats.Readable)} 🥳💖";

                        _giveaway = new(new(), EndsAt: DateTimeOffset.UtcNow + nextRun, _cryptoSecureRandom.GetRandomInt32(config.GiveawayTaypointPrizeMin, config.GiveawayTaypointPrizeMax));
                        var originalMessage = await lounge.SendMessageAsync(
                            text: previousGiveaway?.Winner != null
                                ? getWinnerMessage(previousGiveaway.Winner, previousGiveaway.AmountWon)
                                : null,
                            embed: BuildGiveawayEmbed(),
                            components: builder.Build(),
                            messageReference: previousGiveaway != null ? new(previousGiveaway.OriginalMessageId) : null,
                            allowedMentions: previousGiveaway?.Winner != null ? new AllowedMentions { UserIds = new List<ulong> { previousGiveaway.Winner.UserId.Id } } : null);
                        _giveaway.OriginalMessage = originalMessage;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Exception occurred when attempting to send valentine giveaway.");
                }

                await Task.Delay(nextRun);
            }
        }

        private Embed BuildGiveawayEmbed()
        {
            return new EmbedBuilder()
                .WithColor(new(233, 30, 99))
                .WithTitle("Lover giveaway")
                .WithDescription(string.Join('\n', new[] {
                    "A 💞 lover 💞 taypoint giveaway has started! 🥺",
                    $"💰 Prize: **{_giveaway!.TaypointPrize} taypoints**",
                    $"⌚ Ending: <t:{_giveaway!.EndsAt.ToUnixTimeSeconds()}:R>",
                    $"🧍 Entrants: **{_giveaway!.Entrants.Count}**",
                    $"Enter using the button below! 👉👈",
                }))
                .Build();
        }
    }
}
