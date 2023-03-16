using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;

public interface IModMailBlockedUsersRepository
{
    ValueTask<int> GetBlockedUserCountAsync(IGuild guild);
    ValueTask BlockAsync(IGuild guild, IUser user);
    ValueTask UnblockAsync(IGuild guild, IUser user);
    ValueTask<bool> IsBlockedAsync(IGuild guild, IUser user);
}
