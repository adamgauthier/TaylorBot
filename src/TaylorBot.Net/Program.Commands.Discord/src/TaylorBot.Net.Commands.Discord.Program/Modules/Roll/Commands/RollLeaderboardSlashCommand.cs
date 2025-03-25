using Discord;
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

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Roll.Commands;

public class RollLeaderboardSlashCommand(
    IRollStatsRepository rollStatsRepository,
    MemberNotInGuildUpdater memberNotInGuildUpdater,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "roll leaderboard";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild;

                var leaderboard = await rollStatsRepository.GetLeaderboardAsync(guild);

                if (guild.Fetched != null)
                {
                    memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                        nameof(RollLeaderboardSlashCommand),
                        guild.Fetched,
                        [.. leaderboard.Select(e => new SnowflakeId(e.user_id))]);
                }

                var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(
                    entry => $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"perfect roll".ToQuantity(entry.perfect_roll_count, TaylorBotFormats.BoldReadable)}"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Roll Leaderboard 🍀");

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
                        No roll played by members of this server.
                        Members need to use {mention.SlashCommand("roll play", context)}! 😊
                        """)),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
            ]
        ));
    }
}
