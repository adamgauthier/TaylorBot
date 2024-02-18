using Discord;
using Discord.WebSocket;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Domain;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Taypoints.Commands;

public class TaypointsBalanceSlashCommand(ITaypointBalanceRepository taypointBalanceRepository) : ISlashCommand<TaypointsBalanceSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("taypoints balance");

    public record Options(ParsedUserOrAuthor user);

    public Command Balance(IUser user, bool isLegacyCommand) => new(
        new("taypoints", "Taypoints 🪙", ["points"]),
        async () =>
        {
            TaypointBalance balance;
            if (!user.IsBot && user is IGuildUser guildUser && guildUser.Guild is SocketGuild guild &&
                guild.MemberCount is > 0 and < 10_000)
            {
                balance = await taypointBalanceRepository.GetBalanceWithRankAsync(guildUser);
            }
            else
            {
                balance = await taypointBalanceRepository.GetBalanceAsync(user);
            }

            EmbedBuilder embed = new();

            return new EmbedResult(embed
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
        return new(Balance(options.user.User, isLegacyCommand: false));
    }
}
