using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.Commands.Types;

public interface IUserTracker
{
    ValueTask TrackUserFromArgumentAsync(DiscordUser user);
}

public partial class UserTracker(ILogger<UserTracker> logger, IIgnoredUserRepository ignoredUserRepository, UsernameTrackerDomainService usernameTrackerDomainService, IMemberTrackingRepository memberRepository) : IUserTracker
{
    public async ValueTask TrackUserFromArgumentAsync(DiscordUser user)
    {
        var getUserIgnoreUntilResult = await ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(user, user.IsBot);
        await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(user, getUserIgnoreUntilResult);

        if (user.TryGetMember(out var member))
        {
            var memberAdded = await memberRepository.AddOrUpdateMemberAsync(member, lastSpokeAt: null);

            if (memberAdded)
            {
                LogAddedNewMemberFromArgument(user.FormatLog());
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new member {GuildUser}.")]
    private partial void LogAddedNewMemberFromArgument(string guildUser);
}
