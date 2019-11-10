using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    [RequireNotDisabled]
    public abstract class TaylorBotModuleBase : ModuleBase<TaylorBotShardedCommandContext>
    {
    }
}
