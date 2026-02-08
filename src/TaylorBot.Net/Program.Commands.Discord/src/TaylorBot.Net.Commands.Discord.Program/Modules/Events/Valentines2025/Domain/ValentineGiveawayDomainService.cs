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

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;

public record Giveaway(IList<SnowflakeId> Entrants, DateTimeOffset EndsAt, int TaypointPrize)
{
    public IUserMessage? OriginalMessage { get; set; }
}

public partial class ValentineGiveawayDomainService(
    ILogger<ValentineGiveawayDomainService> logger,
    IValentinesRepository valentinesRepository,
    ITaypointRewardRepository taypointRepository,
    Lazy<ITaylorBotClient> taylorBotClient,
    ICryptoSecureRandom cryptoSecureRandom,
    TimeProvider timeProvider)
{
    public Giveaway? CurrentGiveaway { get; private set; }

    private sealed record PreviousGiveaway(ulong OriginalMessageId)
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
                if (timeProvider.GetUtcNow() < config.GiveawaysEndTime)
                {
                    nextRun = config.TimeSpanBetweenGiveaways;
                    var lounge = (ITextChannel)await taylorBotClient.Value.ResolveRequiredChannelAsync(config.LoungeChannelId);
                    var id = InteractionCustomId.Create(ValentineGiveawayEnterHandler.CustomIdName).RawId;

                    PreviousGiveaway? previousGiveaway = null;

                    if (CurrentGiveaway != null)
                    {
                        if (CurrentGiveaway.OriginalMessage != null)
                        {
                            previousGiveaway = new(CurrentGiveaway.OriginalMessage.Id);

                            var entrants = CurrentGiveaway.Entrants.ToList();
                            if (entrants.Count != 0)
                            {
                                var winnerId = cryptoSecureRandom.GetRandomElement(CurrentGiveaway.Entrants.AsReadOnly());
                                DiscordUser winner = new(winnerId, string.Empty, string.Empty, string.Empty, IsBot: false, null);

                                var rewarded = (await taypointRepository.RewardUsersAsync([winner], CurrentGiveaway.TaypointPrize)).Single();

                                await CurrentGiveaway.OriginalMessage.ModifyAsync(m =>
                                {
                                    m.Embed = EmbedFactory.CreateSuccess(
                                        $"""
                                        This {entrants.Count} entrant giveaway ended!
                                        Congratulations to {MentionUtils.MentionUser(rewarded.UserId.Id)} for winning {"taypoint".ToQuantity(CurrentGiveaway.TaypointPrize, TaylorBotFormats.BoldReadable)}! 🥳💖
                                        """
                                    );
                                    m.Components = new ComponentBuilder().Build();
                                });

                                previousGiveaway.Winner = rewarded;
                                previousGiveaway.AmountWon = CurrentGiveaway.TaypointPrize;
                            }
                            else
                            {
                                await CurrentGiveaway.OriginalMessage.ModifyAsync(m =>
                                {
                                    m.Embed = EmbedFactory.CreateError("Giveaway ended, but there were no entrants 😔");
                                    m.Components = new ComponentBuilder().Build();
                                });
                            }
                        }

                        CurrentGiveaway = null;
                    }

                    var builder = new ComponentBuilder()
                        .WithButton(label: "Enter", customId: id, emote: new Emoji("🎉"));

                    static string GetWinnerMessage(RewardedUserResult r, int won) =>
                        $"{MentionUtils.MentionUser(r.UserId.Id)} You won {"taypoint".ToQuantity(won, TaylorBotFormats.BoldReadable)} from the previous giveaway! You now have {r.NewTaypointCount.ToString(TaylorBotFormats.Readable)} 🥳💖";

                    CurrentGiveaway = new([], EndsAt: timeProvider.GetUtcNow() + nextRun, cryptoSecureRandom.GetInt32(config.GiveawayTaypointPrizeMin, config.GiveawayTaypointPrizeMax));
                    var originalMessage = await lounge.SendMessageAsync(
                        text: previousGiveaway?.Winner != null
                            ? GetWinnerMessage(previousGiveaway.Winner, previousGiveaway.AmountWon)
                            : null,
                        embed: BuildGiveawayEmbed(CurrentGiveaway),
                        components: builder.Build(),
                        messageReference: previousGiveaway != null ? new(previousGiveaway.OriginalMessageId) : null,
                        allowedMentions: previousGiveaway?.Winner != null ? new AllowedMentions { UserIds = [previousGiveaway.Winner.UserId.Id] } : null);
                    CurrentGiveaway.OriginalMessage = originalMessage;
                }
            }
            catch (Exception exception)
            {
                LogExceptionSendingValentineGiveaway(exception);
            }

            await Task.Delay(nextRun);
        }
    }

    public static Embed BuildGiveawayEmbed(Giveaway giveaway)
    {
        return new EmbedBuilder()
            .WithColor(new(233, 30, 99))
            .WithTitle("Lover Giveaway 💝")
            .WithDescription(
                $"""
                A 💞 lover 💞 taypoint giveaway has started! 🥺
                💰 Prize: **{giveaway.TaypointPrize} taypoints**
                ⌚ Ending: <t:{giveaway.EndsAt.ToUnixTimeSeconds()}:R>
                🧍 Entrants: **{giveaway.Entrants.Count}**
                Enter using the button below! 👉👈
                """)
            .Build();
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when attempting to send valentine giveaway.")]
    private partial void LogExceptionSendingValentineGiveaway(Exception exception);
}

public class ValentineGiveawayEnterHandler(
    IValentinesRepository valentinesRepository,
    CommandMentioner mention,
    IInteractionResponseClient interactionResponseClient,
    ValentineGiveawayDomainService giveawayService) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ValentineGiveawayEnter;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText());

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var config = await valentinesRepository.GetConfigurationAsync();
        var giveaway = giveawayService.CurrentGiveaway;

        if (giveaway != null && !giveaway.Entrants.Contains(button.Interaction.UserId))
        {
            var given = await valentinesRepository.GetRoleObtainedFromUserAsync(context.User);
            if (given.Count >= config.SpreadLimit)
            {
                giveaway.Entrants.Add(button.Interaction.UserId);

                await interactionResponseClient.EditOriginalResponseAsync(button.Interaction, message: new(
                    new([ValentineGiveawayDomainService.BuildGiveawayEmbed(giveaway)], Content: giveaway.OriginalMessage?.Content ?? ""),
                    [new Button("enter-giveaway", ButtonStyle.Primary, "Enter", "🎉")]
                ));

                await Task.Delay(100);

                await interactionResponseClient.SendFollowupResponseAsync(button.Interaction,
                    new(new(EmbedFactory.CreateSuccess("You are now entered into this giveaway! 🗳️")), IsPrivate: true));
            }
            else
            {
                await interactionResponseClient.SendFollowupResponseAsync(button.Interaction,
                    new(new(EmbedFactory.CreateError(
                        $"""
                        You can't enter because you haven't spread love to **{config.SpreadLimit}** people yet ({given.Count}/{config.SpreadLimit})! ⛔
                        Please use {mention.GuildSlashCommand("love spread", context.Guild?.Id ?? throw new InvalidOperationException())} to give the role to someone who doesn't have it 😊
                        """)
                    ), IsPrivate: true));
            }
        }
    }
}
