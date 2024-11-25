using System.Reflection;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IOngoingCommandRepository
{
    ValueTask<bool> HasAnyOngoingCommandAsync(DiscordUser user, string pool);
    ValueTask AddOngoingCommandAsync(DiscordUser user, string pool);
    ValueTask RemoveOngoingCommandAsync(DiscordUser user, string pool);
}

public class UserNoOngoingCommandPrecondition(IOngoingCommandRepository ongoingCommandRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var pool = command.Metadata.Name is SharedCommands.Help ?
            $"help.{Assembly.GetEntryAssembly()!.GetName().Name!.ToLowerInvariant()}" :
            string.Empty;

        var hasAnyOngoingCommand = await ongoingCommandRepository.HasAnyOngoingCommandAsync(context.User, pool);

        if (hasAnyOngoingCommand)
        {
            return new PreconditionFailed(
                PrivateReason: "user has an ongoing command",
                UserReason: new($"You can't use {context.MentionCommand(command)} because you have an ongoing command.", HideInPrefixCommands: true)
            );
        }
        else
        {
            await ongoingCommandRepository.AddOngoingCommandAsync(context.User, pool);
            context.OnGoing.OnGoingCommandAddedToPool = pool;
            return new PreconditionPassed();
        }
    }
}
