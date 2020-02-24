using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    [RequireNotDisabled]
    [RequireNotGuildDisabled]
    [RequireNotGuildChannelDisabled]
    [RequireUserNotIgnored]
    [RequireMemberTracked]
    [RequireUserNoOngoingCommandAtttribute]
    public abstract class TaylorBotModule : ModuleBase<TaylorBotShardedCommandContext>
    {
    }
}
