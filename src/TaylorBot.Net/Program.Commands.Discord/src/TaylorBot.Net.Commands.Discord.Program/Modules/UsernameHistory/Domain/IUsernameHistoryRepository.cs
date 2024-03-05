using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;

public record UsernameChange(string Username, DateTimeOffset ChangedAt);

public interface IUsernameHistoryRepository
{
    ValueTask<IReadOnlyList<UsernameChange>> GetUsernameHistoryFor(IUser user, int count);

    ValueTask<bool> IsUsernameHistoryHiddenFor(IUser user);

    ValueTask HideUsernameHistoryFor(IUser user);

    ValueTask UnhideUsernameHistoryFor(IUser user);
}
