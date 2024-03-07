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

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsBalanceSlashCommand(ITaypointBalanceRepository taypointBalanceRepository, TaypointGuildCacheUpdater taypointGuildCacheUpdater) : ISlashCommand<TaypointsBalanceSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints balance");

    public record Options(ParsedUserOrAuthor user);

    public Command Balance(IUser user) => new(
        new("taypoints", "Taypoints 🪙", ["points"]),
        async () =>
        {
            var balance = await taypointBalanceRepository.GetBalanceAsync(user);

            taypointGuildCacheUpdater.UpdateLastKnownPointCountInBackground(user, balance.TaypointCount);

            return new EmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(user)
                .WithDescription(
                    $"""
                    {user.Mention}'s balance is {"taypoint".ToQuantity(balance.TaypointCount, TaylorBotFormats.BoldReadable)} 🪙
                    {(balance.ServerRank.HasValue ? $"They are ranked **{balance.ServerRank.Value.Ordinalize(TaylorBotCulture.Culture)}** in this server (excluding users who left)" : "")}
                    """)
            .Build());
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Balance(options.user.User));
    }
}
