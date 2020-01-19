using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    [RequireNotDisabled]
    [RequireNotGuildDisabled]
    [RequireNotGuildChannelDisabled]
    [RequireUserNotIgnored]
    public abstract class TaylorBotModule : ModuleBase<TaylorBotShardedCommandContext>
    {
    }
}
