using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Types
{
    public interface IUserArgument<T> where T : class, IUser
    {
        ulong UserId { get; }
        ValueTask<T> GetTrackedUserAsync();
    }

    public class UserArgument<T> : IUserArgument<T>, IMentionedUser<T>, IMentionedUserNotAuthor<T>, IMentionedUserNotAuthorOrClient<T>
        where T : class, IUser
    {
        private readonly T _user;
        private readonly IUserTracker _userTracker;

        public UserArgument(T user, IUserTracker userTracker)
        {
            _user = user;
            _userTracker = userTracker;
        }

        public ulong UserId => _user.Id;

        public async ValueTask<T> GetTrackedUserAsync()
        {
            await _userTracker.TrackUserFromArgumentAsync(_user);

            return _user;
        }
    }
}
