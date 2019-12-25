using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    [RequireNotDisabled]
    [RequireNotGuildDisabled]
    [RequireNotGuildChannelDisabled]
    public abstract class TaylorBotModule : ModuleBase<TaylorBotShardedCommandContext>
    {
    }
}
