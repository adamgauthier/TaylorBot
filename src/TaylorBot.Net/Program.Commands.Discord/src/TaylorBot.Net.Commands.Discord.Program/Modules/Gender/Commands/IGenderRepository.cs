using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public interface IGenderRepository
{
    ValueTask<TextAttributeValue?> GetGenderAsync(DiscordUser user);
    ValueTask SetGenderAsync(DiscordUser user, string gender);
    ValueTask ClearGenderAsync(DiscordUser user);
}
