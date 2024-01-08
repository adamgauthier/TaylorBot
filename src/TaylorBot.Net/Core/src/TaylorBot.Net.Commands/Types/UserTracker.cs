using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.Commands.Types;

public interface IUserTracker
{
    ValueTask TrackUserFromArgumentAsync(IUser user);
}

public class UserTracker(ILogger<UserTracker> logger, IIgnoredUserRepository ignoredUserRepository, UsernameTrackerDomainService usernameTrackerDomainService, IMemberTrackingRepository memberRepository) : IUserTracker
{
    public async ValueTask TrackUserFromArgumentAsync(IUser user)
    {
        var getUserIgnoreUntilResult = await ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(user, user.IsBot);
        await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(user, getUserIgnoreUntilResult);

        if (user is IGuildUser guildUser)
        {
            var memberAdded = await memberRepository.AddOrUpdateMemberAsync(guildUser, lastSpokeAt: null);

            if (memberAdded)
            {
                logger.LogInformation($"Added new member {guildUser.FormatLog()}.");
            }
        }
    }
}
