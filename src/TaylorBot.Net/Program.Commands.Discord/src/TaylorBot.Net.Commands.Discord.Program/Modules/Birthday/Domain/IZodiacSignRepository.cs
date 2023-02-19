using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain
{
    public interface IZodiacSignRepository
    {
        ValueTask<string?> GetZodiacForUserAsync(IUser user);
    }
}
