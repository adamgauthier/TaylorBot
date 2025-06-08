using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IOngoingCommandRepository
{
    ValueTask<bool> HasAnyOngoingCommandAsync(DiscordUser user, string pool);
    ValueTask AddOngoingCommandAsync(DiscordUser user, string pool);
    ValueTask RemoveOngoingCommandAsync(DiscordUser user, string pool);
}

public class UserNoOngoingCommandPrecondition(IOngoingCommandRepository ongoingCommandRepository, CommandMentioner mention) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var pool = string.Empty;

        var hasAnyOngoingCommand = await ongoingCommandRepository.HasAnyOngoingCommandAsync(context.User, pool);

        if (hasAnyOngoingCommand)
        {
            return new PreconditionFailed(
                PrivateReason: "user has an ongoing command",
                UserReason: new($"You can't use {mention.Command(command, context)} because you have an ongoing command.", HideInPrefixCommands: true)
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
