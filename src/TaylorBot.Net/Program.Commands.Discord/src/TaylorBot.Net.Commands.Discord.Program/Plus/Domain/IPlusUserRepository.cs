using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Plus.Domain
{
    public record PlusUser(bool IsActive, int MaxPlusGuilds, IReadOnlyCollection<string> ActivePlusGuilds);

    public interface IPlusUserRepository
    {
        ValueTask<PlusUser?> GetPlusUserAsync(IUser user);
        ValueTask AddPlusGuildAsync(IUser user, IGuild guild);
        ValueTask DisablePlusGuildAsync(IUser user, IGuild guild);
    }
}
