using Humanizer;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.Commands.Preconditions;

public record GetUserIgnoreUntilResult(DateTimeOffset IgnoreUntil, bool WasAdded, bool WasUsernameChanged, string? PreviousUsername) : IUserAddedResult;

public interface IIgnoredUserRepository
{
    ValueTask<GetUserIgnoreUntilResult> InsertOrGetUserIgnoreUntilAsync(DiscordUser user, bool isBot);
    ValueTask IgnoreUntilAsync(DiscordUser user, DateTimeOffset until);
}

public class UserNotIgnoredPrecondition(IIgnoredUserRepository ignoredUserRepository, UsernameTrackerDomainService usernameTrackerDomainService) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var getUserIgnoreUntilResult = await ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(context.User, isBot: false);

        await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(context.User, getUserIgnoreUntilResult);

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
