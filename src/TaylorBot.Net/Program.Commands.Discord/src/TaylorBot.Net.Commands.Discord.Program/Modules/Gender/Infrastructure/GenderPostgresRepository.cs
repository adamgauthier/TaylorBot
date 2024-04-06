using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Infrastructure;

public class GenderPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IGenderRepository
{
    public ValueTask<string?> GetGenderAsync(DiscordUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "gender");

    public ValueTask SetGenderAsync(DiscordUser user, string gender) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "gender", gender);

    public ValueTask ClearGenderAsync(DiscordUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "gender");
}
