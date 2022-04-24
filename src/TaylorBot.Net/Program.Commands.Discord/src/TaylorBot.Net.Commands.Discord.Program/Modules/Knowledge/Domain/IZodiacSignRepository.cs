using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Domain
{
    public interface IZodiacSignRepository
    {
        ValueTask<string?> GetZodiacForUserAsync(IUser user);
    }
}
