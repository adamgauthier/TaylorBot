using Discord;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;

public interface IModMailBlockedUsersRepository
{
    ValueTask<int> GetBlockedUserCountAsync(IGuild guild);
    ValueTask BlockAsync(IGuild guild, DiscordUser user);
    ValueTask UnblockAsync(IGuild guild, DiscordUser user);
    ValueTask<bool> IsBlockedAsync(IGuild guild, DiscordUser user);
}
