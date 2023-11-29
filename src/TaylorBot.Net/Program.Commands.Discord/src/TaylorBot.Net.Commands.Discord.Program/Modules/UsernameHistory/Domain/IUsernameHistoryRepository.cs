using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain
{
    public interface IUsernameHistoryRepository
    {
        record UsernameChange(string Username, DateTimeOffset ChangedAt);

        ValueTask<IReadOnlyList<UsernameChange>> GetUsernameHistoryFor(IUser user, int count);

        ValueTask<bool> IsUsernameHistoryHiddenFor(IUser user);

        ValueTask HideUsernameHistoryFor(IUser user);

        ValueTask UnhideUsernameHistoryFor(IUser user);
    }
}
