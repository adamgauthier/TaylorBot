using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

public class DailyLeaderboardSlashCommand(IDailyPayoutRepository dailyPayoutRepository, MemberNotInGuildUpdater memberNotInGuildUpdater) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("daily leaderboard");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild;

                var leaderboard = await dailyPayoutRepository.GetLeaderboardAsync(guild);

                if (guild.Fetched != null)
                {
                    memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                        nameof(DailyLeaderboardSlashCommand),
                        guild.Fetched,
                        leaderboard.Select(e => e.UserId).ToList());
                }

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.Rank}\\. {entry.Username.MdUserLink(entry.UserId)}: {"day".ToQuantity(entry.CurrentDailyStreak, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Daily Streak Leaderboard 📅");

                if (guild.Fetched != null)
                {
                    baseEmbed.WithGuildAsAuthor(guild.Fetched);
                }

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText:
                        $"""
                        No daily streaks in this server.
                        Members need to use {context.MentionCommand("daily claim")}! 😊
                        """)),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
            ]
        ));
    }
}
