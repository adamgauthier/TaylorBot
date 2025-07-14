using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands;

[Name("Daily Payout 👔")]
public class DailyPayoutModule(ICommandRunner commandRunner, DailyClaimSlashCommand dailyClaimCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("daily")]
    [Alias("dailypayout")]
    public async Task<RuntimeResult> DailyAsync()
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: DailyClaimSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            dailyClaimCommand.Claim(context.User, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("dailystreak")]
    [Alias("dstreak")]
    public async Task<RuntimeResult> StreakAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: DailyStreakSlashCommand.CommandName, IsRemoved: true));

    [Command("rankdailystreak")]
    [Alias("rank dailystreak", "rankdstreak", "rank dstreak")]
    public async Task<RuntimeResult> LeaderboardAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: DailyLeaderboardSlashCommand.CommandName, IsRemoved: true));
}
