using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IOngoingCommandRepository
    {
        Task<bool> HasAnyOngoingCommandAsync(IUser user);
        Task AddOngoingCommandAsync(IUser user);
        Task RemoveOngoingCommandAsync(IUser user);
    }

    public class RequireUserNoOngoingCommandAtttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ongoingCommandRepository = services.GetRequiredService<IOngoingCommandRepository>();

            var hasAnyOngoingCommand = await ongoingCommandRepository.HasAnyOngoingCommandAsync(context.User);

            if (hasAnyOngoingCommand)
            {
                return TaylorBotPreconditionResult.FromPrivateError("user has an ongoing command");
            }
            else
            {
                await ongoingCommandRepository.AddOngoingCommandAsync(context.User);
                ((TaylorBotShardedCommandContext)context).WasOnGoingCommandAdded = true;
                return PreconditionResult.FromSuccess();
            }
        }
    }
}
