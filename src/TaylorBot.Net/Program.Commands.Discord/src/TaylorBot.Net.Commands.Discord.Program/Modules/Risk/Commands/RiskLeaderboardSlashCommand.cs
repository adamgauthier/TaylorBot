﻿using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Risk.Commands;

public class RiskLeaderboardSlashCommand(IRiskStatsRepository riskStatsRepository, MemberNotInGuildUpdater memberNotInGuildUpdater) : ISlashCommand<NoOptions>
{
    public static string CommandName => "risk leaderboard";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild;

                var leaderboard = await riskStatsRepository.GetLeaderboardAsync(guild);

                if (guild.Fetched != null)
                {
                    memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                        nameof(RiskLeaderboardSlashCommand),
                        guild.Fetched,
                        leaderboard.Select(e => new SnowflakeId(e.user_id)).ToList());
                }

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"win".ToQuantity(entry.gamble_win_count, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Risk Wins Leaderboard 💼");

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
                        No risks played by members of this server.
                        Members need to use {context.MentionSlashCommand("risk play")}! 😊
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
