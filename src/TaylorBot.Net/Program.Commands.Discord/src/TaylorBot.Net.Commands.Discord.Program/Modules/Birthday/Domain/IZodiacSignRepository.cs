using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

public interface IZodiacSignRepository
{
    ValueTask<string?> GetZodiacForUserAsync(DiscordUser user);
}
