using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UsernameHistory.Domain;

public record UsernameChange(string Username, DateTimeOffset ChangedAt);

public interface IUsernameHistoryRepository
{
    ValueTask<IReadOnlyList<UsernameChange>> GetUsernameHistoryFor(DiscordUser user, int count);

    ValueTask<bool> IsUsernameHistoryHiddenFor(DiscordUser user);

    ValueTask HideUsernameHistoryFor(DiscordUser user);

    ValueTask UnhideUsernameHistoryFor(DiscordUser user);
}
