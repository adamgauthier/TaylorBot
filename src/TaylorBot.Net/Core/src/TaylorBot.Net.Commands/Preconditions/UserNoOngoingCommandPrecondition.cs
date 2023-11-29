using Discord;
using System.Reflection;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IOngoingCommandRepository
    {
        ValueTask<bool> HasAnyOngoingCommandAsync(IUser user, string pool);
        ValueTask AddOngoingCommandAsync(IUser user, string pool);
        ValueTask RemoveOngoingCommandAsync(IUser user, string pool);
    }

    public class UserNoOngoingCommandPrecondition : ICommandPrecondition
    {
        private readonly IOngoingCommandRepository _ongoingCommandRepository;

        public UserNoOngoingCommandPrecondition(IOngoingCommandRepository ongoingCommandRepository)
        {
            _ongoingCommandRepository = ongoingCommandRepository;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            var pool = command.Metadata.Name is SharedCommands.Help or SharedCommands.Diagnostic ?
                $"help.{Assembly.GetEntryAssembly()!.GetName().Name!.ToLowerInvariant()}" :
                string.Empty;

            var hasAnyOngoingCommand = await _ongoingCommandRepository.HasAnyOngoingCommandAsync(context.User, pool);

            if (hasAnyOngoingCommand)
            {
                return new PreconditionFailed(
                    PrivateReason: "user has an ongoing command",
                    UserReason: new($"You can't use `{command.Metadata.Name}` because you have an ongoing command.", HideInPrefixCommands: true)
                );
            }
            else
            {
                await _ongoingCommandRepository.AddOngoingCommandAsync(context.User, pool);
                context.OnGoing.OnGoingCommandAddedToPool = pool;
                return new PreconditionPassed();
            }
        }
    }
}
