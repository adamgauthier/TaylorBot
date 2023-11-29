using Discord.Commands;

namespace TaylorBot.Net.Commands.DiscordNet
{
    public class RequireCommandInfoSetAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            ((ITaylorBotCommandContext)context).CommandInfos.Add(command);
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    [RequireCommandInfoSet]
    public abstract class TaylorBotModule : ModuleBase<ITaylorBotCommandContext>
    {
    }
}
