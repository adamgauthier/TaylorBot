using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Infrastructure;

public class GenderPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IGenderRepository
{
    public ValueTask<string?> GetGenderAsync(IUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "gender");

    public ValueTask SetGenderAsync(IUser user, string gender) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "gender", gender);

    public ValueTask ClearGenderAsync(IUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "gender");
}
