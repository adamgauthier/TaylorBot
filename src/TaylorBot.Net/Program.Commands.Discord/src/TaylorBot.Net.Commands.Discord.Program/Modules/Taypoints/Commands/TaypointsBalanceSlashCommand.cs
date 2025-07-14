using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public record TaypointBalance(long TaypointCount, int? ServerRank);

public class TaypointsBalanceSlashCommand(
    ITaypointBalanceRepository taypointBalanceRepository,
    TaypointGuildCacheUpdater taypointGuildCacheUpdater,
    TaskExceptionLogger taskExceptionLogger,
    CommandMentioner mention) : ISlashCommand<TaypointsBalanceSlashCommand.Options>
{
    public static string CommandName => "taypoints balance";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedUserOrAuthor user);

    public Command Balance(DiscordUser user, RunContext context) => new(
        new("taypoints", ["points"], IsSlashCommand: context.SlashCommand != null),
        async () =>
        {
            TaypointBalance balance = new(
                await taypointBalanceRepository.GetBalanceAsync(user),
                await GetServerRankAsync(user, context.Guild));

            UpdateLastKnowPointCountInBackground(user, balance);

            return new EmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(user)
                .WithDescription(
                    $"""
                    {user.Mention} has {"taypoint".ToQuantity(balance.TaypointCount, TaylorBotFormats.BoldReadable)} 🪙
                    {(balance.ServerRank != null ? GetRankText(context, balance.ServerRank.Value) : "")}
                    """)
            .Build());
        }
    );

    private string GetRankText(RunContext context, int serverRank)
    {
        var emoji = serverRank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => "🏆",
        };

        return $"{emoji} **{serverRank.Ordinalize(TaylorBotCulture.Culture)}** in this server's {mention.SlashCommand("taypoints leaderboard", context)}";
    }

    private async Task<int?> GetServerRankAsync(DiscordUser user, CommandGuild? guild)
    {
        if (guild == null)
        {
            return null;
        }

        var leaderboard = await taypointBalanceRepository.GetLeaderboardAsync(guild);
        return (int?)leaderboard.SingleOrDefault(e => e.user_id == user.Id)?.rank;
    }

    private void UpdateLastKnowPointCountInBackground(DiscordUser user, TaypointBalance balance)
    {
        _ = taskExceptionLogger.LogOnError(
            async () => await taypointGuildCacheUpdater.UpdateLastKnownPointCountAsync(user, balance.TaypointCount),
            nameof(UpdateLastKnowPointCountInBackground)
        );
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Balance(options.user.User, context));
    }
}
