using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;

namespace TaylorBot.Net.Commands
{
    [RequireNotDisabled]
    [RequireNotGuildDisabled]
    [RequireNotGuildChannelDisabled]
    [RequireUserNotIgnored]
    [RequireMemberTracked]
    [RequireTextChannelTracked]
    [RequireUserNoOngoingCommand]
    public abstract class TaylorBotModule : ModuleBase<ITaylorBotCommandContext>
    {
    }
}
