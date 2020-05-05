using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class RequireInGuildAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null)
            {
                return Task.FromResult<PreconditionResult>(TaylorBotPreconditionResult.FromUserError(
                    privateReason: $"{command.Name} can only be used in a guild",
                    userReason: $"You can't use `{command.Name}` because it can only be used in a server."
                ));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
