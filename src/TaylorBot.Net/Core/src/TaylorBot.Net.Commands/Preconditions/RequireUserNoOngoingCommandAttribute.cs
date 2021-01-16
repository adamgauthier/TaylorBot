using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Modules;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IOngoingCommandRepository
    {
        ValueTask<bool> HasAnyOngoingCommandAsync(IUser user, string pool);
        ValueTask AddOngoingCommandAsync(IUser user, string pool);
        ValueTask RemoveOngoingCommandAsync(IUser user, string pool);
    }

    public class RequireUserNoOngoingCommandAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ongoingCommandRepository = services.GetRequiredService<IOngoingCommandRepository>();

            var pool = command.Module.Name == ModuleNames.Help ?
                $"help.{Assembly.GetEntryAssembly()!.GetName().Name!.ToLowerInvariant()}" :
                string.Empty;

            var hasAnyOngoingCommand = await ongoingCommandRepository.HasAnyOngoingCommandAsync(context.User, pool);

            if (hasAnyOngoingCommand)
            {
                return TaylorBotPreconditionResult.FromPrivateError("user has an ongoing command");
            }
            else
            {
                await ongoingCommandRepository.AddOngoingCommandAsync(context.User, pool);
                ((TaylorBotShardedCommandContext)context).OnGoingCommandAddedToPool = pool;
                return PreconditionResult.FromSuccess();
            }
        }
    }
}
