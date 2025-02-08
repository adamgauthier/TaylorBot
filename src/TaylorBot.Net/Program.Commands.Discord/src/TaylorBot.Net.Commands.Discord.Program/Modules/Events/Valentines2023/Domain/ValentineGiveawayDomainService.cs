using Discord;
using Humanizer;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Discord.Program.Modules.TaypointReward.Domain;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Random;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2023.Domain;

public record Giveaway(List<SnowflakeId> Entrants, DateTimeOffset EndsAt, int TaypointPrize)
{
    public IUserMessage? OriginalMessage { get; set; } = null;
}

public class ValentineGiveawayDomainService(
    ILogger<ValentineGiveawayDomainService> logger,
    IValentinesRepository valentinesRepository,
    ITaypointRewardRepository taypointRepository,
    ITaylorBotClient taylorBotClient,
    MessageComponentHandler messageComponentHandler,
    InteractionResponseClient interactionResponseClient,
    ICryptoSecureRandom cryptoSecureRandom
    )
{
    private Giveaway? _giveaway = null;

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
                var config = await valentinesRepository.GetConfigurationAsync();
                if (DateTimeOffset.UtcNow < config.GiveawaysEndTime)
                {
                    nextRun = config.TimeSpanBetweenGiveaways;
                    var lounge = (ITextChannel)await taylorBotClient.ResolveRequiredChannelAsync(config.LoungeChannelId);
                    var id = "enter-giveaway";

                    PreviousGiveaway? previousGiveaway = null;

                    if (_giveaway != null)
                    {
                        messageComponentHandler.RemoveCallback(id);

                        if (_giveaway.OriginalMessage != null)
                        {
                            previousGiveaway = new(_giveaway.OriginalMessage.Id);

                            var entrants = _giveaway.Entrants.ToList();
                            if (entrants.Any())
                            {
                                var winnerId = cryptoSecureRandom.GetRandomElement(_giveaway.Entrants);
                                var winner = new DiscordUser(winnerId, string.Empty, string.Empty, string.Empty, IsBot: false, null);

                                var rewarded = (await taypointRepository.RewardUsersAsync([winner], _giveaway.TaypointPrize)).Single();

                                await _giveaway.OriginalMessage.ModifyAsync(m =>
                                {
                                    m.Embed = EmbedFactory.CreateSuccess(
                                        $"""
                                        This {entrants.Count} entrant giveaway ended!
                                        Congratulations to {MentionUtils.MentionUser(rewarded.UserId.Id)} for winning {"taypoint".ToQuantity(_giveaway.TaypointPrize, TaylorBotFormats.BoldReadable)}! 🥳💖
                                        """
                                    );
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

                    messageComponentHandler.AddCallback(id, async component =>
                    {
                        try
                        {
                            if (_giveaway != null && !_giveaway.Entrants.Contains(component.Interaction.UserId))
                            {
                                var user = new DiscordUser(component.Interaction.UserId, string.Empty, string.Empty, string.Empty, IsBot: false, null);
                                var given = await valentinesRepository.GetRoleObtainedFromUserAsync(user);

                                if (given.Count >= config.SpreadLimit)
                                {
                                    _giveaway.Entrants.Add(component.Interaction.UserId);

                                    await interactionResponseClient.EditOriginalResponseAsync(component.Interaction, message: new(
                                        new([BuildGiveawayEmbed()], Content: _giveaway?.OriginalMessage?.Content ?? ""),
                                        [new Button("enter-giveaway", ButtonStyle.Primary, "Enter giveaway", "🎉")]
                                    ));

                                    await interactionResponseClient.SendFollowupResponseAsync(component.Interaction,
                                        new(new(EmbedFactory.CreateSuccess("You are now entered into this giveaway! 🗳️")), IsPrivate: true));
                                }
                                else
                                {
                                    await interactionResponseClient.SendFollowupResponseAsync(component.Interaction,
                                        new(new(EmbedFactory.CreateError(
                                            $"""
                                            You can't enter a giveaway because you haven't spread love to **{config.SpreadLimit}** people yet ({given.Count}/{config.SpreadLimit})! ⛔
                                            Please use /love spread to give the role to someone who doesn't have it yet. 😊
                                            """)
                                        ), IsPrivate: true));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, $"Unhandled exception in button {id}:");
                            await interactionResponseClient.SendFollowupResponseAsync(component.Interaction,
                                new(new(EmbedFactory.CreateError("Oops, an unknown error occurred. Sorry about that. 😕")), IsPrivate: true));
                        }
                    });

                    var getWinnerMessage = (RewardedUserResult r, int won) =>
                        $"{MentionUtils.MentionUser(r.UserId.Id)} You won {"taypoint".ToQuantity(won, TaylorBotFormats.BoldReadable)} from the previous giveaway! You now have {r.NewTaypointCount.ToString(TaylorBotFormats.Readable)} 🥳💖";

                    _giveaway = new([], EndsAt: DateTimeOffset.UtcNow + nextRun, cryptoSecureRandom.GetInt32(config.GiveawayTaypointPrizeMin, config.GiveawayTaypointPrizeMax));
                    var originalMessage = await lounge.SendMessageAsync(
                        text: previousGiveaway?.Winner != null
                            ? getWinnerMessage(previousGiveaway.Winner, previousGiveaway.AmountWon)
                            : null,
                        embed: BuildGiveawayEmbed(),
                        components: builder.Build(),
                        messageReference: previousGiveaway != null ? new(previousGiveaway.OriginalMessageId) : null,
                        allowedMentions: previousGiveaway?.Winner != null ? new AllowedMentions { UserIds = [previousGiveaway.Winner.UserId.Id] } : null);
                    _giveaway.OriginalMessage = originalMessage;
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when attempting to send valentine giveaway.");
            }

            await Task.Delay(nextRun);
        }
    }

    private Embed BuildGiveawayEmbed()
    {
        return new EmbedBuilder()
            .WithColor(new(233, 30, 99))
            .WithTitle("Lover giveaway")
            .WithDescription(
                $"""
                A 💞 lover 💞 taypoint giveaway has started! 🥺
                💰 Prize: **{_giveaway!.TaypointPrize} taypoints**
                ⌚ Ending: <t:{_giveaway!.EndsAt.ToUnixTimeSeconds()}:R>
                🧍 Entrants: **{_giveaway!.Entrants.Count}**
                Enter using the button below! 👉👈
                """)
            .Build();
    }
}
