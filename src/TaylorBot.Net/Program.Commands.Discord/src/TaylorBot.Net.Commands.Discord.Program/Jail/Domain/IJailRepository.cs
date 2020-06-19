using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Jail.Domain
{
    public interface IJailRepository
    {
        ValueTask SetJailRoleAsync(IGuild guild, IRole jailRole);
    }
}
