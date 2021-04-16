using Discord;
using Humanizer;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.Commands.Preconditions
{
    public record GetUserIgnoreUntilResult(DateTimeOffset IgnoreUntil, bool WasAdded, bool WasUsernameChanged, string? PreviousUsername) : IUserAddedResult;

    public interface IIgnoredUserRepository
    {
        ValueTask<GetUserIgnoreUntilResult> InsertOrGetUserIgnoreUntilAsync(IUser user);
        ValueTask IgnoreUntilAsync(IUser user, DateTimeOffset until);
    }

    public class UserNotIgnoredPrecondition : ICommandPrecondition
    {
        private readonly IIgnoredUserRepository _ignoredUserRepository;
        private readonly UsernameTrackerDomainService _usernameTrackerDomainService;

        public UserNotIgnoredPrecondition(IIgnoredUserRepository ignoredUserRepository, UsernameTrackerDomainService usernameTrackerDomainService)
        {
            _ignoredUserRepository = ignoredUserRepository;
            _usernameTrackerDomainService = usernameTrackerDomainService;
        }

        public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
        {
            var getUserIgnoreUntilResult = await _ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(context.User);

            await _usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(context.User, getUserIgnoreUntilResult);

            return DateTimeOffset.Now < getUserIgnoreUntilResult.IgnoreUntil ?
                new PreconditionFailed(
                    PrivateReason: $"user is ignored until {getUserIgnoreUntilResult.IgnoreUntil:o}",
                    UserReason: new(
                        $"You can't use `{command.Metadata.Name}` because you are ignored until {getUserIgnoreUntilResult.IgnoreUntil.Humanize(culture: TaylorBotCulture.Culture)}.",
                        HideInPrefixCommands: true
                    )
                ) :
                new PreconditionPassed();
        }
    }
}
