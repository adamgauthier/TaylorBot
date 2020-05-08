using Discord.Commands;
using System;
using System.Linq;
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
                    privateReason: $"{command.Aliases.First()} can only be used in a guild",
                    userReason: $"You can't use `{command.Aliases.First()}` because it can only be used in a server."
                ));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
