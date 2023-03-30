using Discord;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain;

namespace TaylorBot.Net.Commands.Types
{
    public interface IUserTracker
    {
        ValueTask TrackUserFromArgumentAsync(IUser user);
    }

    public class UserTracker : IUserTracker
    {
        private readonly ILogger<UserTracker> _logger;
        private readonly IIgnoredUserRepository _ignoredUserRepository;
        private readonly UsernameTrackerDomainService _usernameTrackerDomainService;
        private readonly IMemberRepository _memberRepository;

        public UserTracker(ILogger<UserTracker> logger, IIgnoredUserRepository ignoredUserRepository, UsernameTrackerDomainService usernameTrackerDomainService, IMemberRepository memberRepository)
        {
            _logger = logger;
            _ignoredUserRepository = ignoredUserRepository;
            _usernameTrackerDomainService = usernameTrackerDomainService;
            _memberRepository = memberRepository;
        }

        public async ValueTask TrackUserFromArgumentAsync(IUser user)
        {
            var getUserIgnoreUntilResult = await _ignoredUserRepository.InsertOrGetUserIgnoreUntilAsync(user, user.IsBot);
            await _usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(user, getUserIgnoreUntilResult);

            if (user is IGuildUser guildUser)
            {
                var memberAdded = await _memberRepository.AddOrUpdateMemberAsync(guildUser, lastSpokeAt: null);

                if (memberAdded)
                {
                    _logger.LogInformation($"Added new member {guildUser.FormatLog()}.");
                }
            }
        }
    }
}
