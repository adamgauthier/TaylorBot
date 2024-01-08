using Discord;

namespace TaylorBot.Net.Commands.Types;

public interface IUserArgument<T> where T : class, IUser
{
    ulong UserId { get; }
    ValueTask<T> GetTrackedUserAsync();
}

public class UserArgument<T>(T user, IUserTracker userTracker) : IUserArgument<T>, IMentionedUser<T>, IMentionedUserNotAuthor<T>, IMentionedUserNotAuthorOrClient<T>
    where T : class, IUser
{
    public ulong UserId => user.Id;

    public async ValueTask<T> GetTrackedUserAsync()
    {
        await userTracker.TrackUserFromArgumentAsync(user);

        return user;
    }
}
