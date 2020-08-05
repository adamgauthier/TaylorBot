using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class RequireTaylorBotOwnerAttribute : PreconditionAttribute
    {
        private readonly RequireOwnerAttribute _requireOwner = new RequireOwnerAttribute();

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ownerResult = await _requireOwner.CheckPermissionsAsync(context, command, services);
            if (ownerResult.IsSuccess)
            {
                return PreconditionResult.FromSuccess();
            }
            else
            {
                var commandName = command.Aliases.First();
                return TaylorBotPreconditionResult.FromUserError(
                    privateReason: $"{commandName} can only be used by owner",
                    userReason: $"You can't use `{commandName}` because it can only be used by the bot owner."
                );
            }
        }
    }
}
