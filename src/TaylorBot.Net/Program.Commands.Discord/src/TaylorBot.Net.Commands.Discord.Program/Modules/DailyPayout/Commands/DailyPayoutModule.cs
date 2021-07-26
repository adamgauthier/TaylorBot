using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Commands
{
    [Name("Daily Payout 👔")]
    [Group("daily")]
    [Alias("dailypayout")]
    public class DailyPayoutModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly DailyClaimCommand _dailyClaimCommand;

        public DailyPayoutModule(ICommandRunner commandRunner, DailyClaimCommand dailyClaimCommand)
        {
            _commandRunner = commandRunner;
            _dailyClaimCommand = dailyClaimCommand;
        }

        [Command]
        [Summary("Awards you with your daily amount of taypoints.")]
        public async Task<RuntimeResult> DailyAsync()
        {
            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(
                _dailyClaimCommand.Claim(Context.User, Context.CommandPrefix, isLegacyCommand: true),
                context
            );

            return new TaylorBotResult(result, context);
        }
    }
}

