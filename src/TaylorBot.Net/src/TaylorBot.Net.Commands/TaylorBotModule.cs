using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    [RequireNotDisabled]
    [RequireNotGuildDisabled]
    public abstract class TaylorBotModule : ModuleBase<TaylorBotShardedCommandContext>
    {
    }
}
