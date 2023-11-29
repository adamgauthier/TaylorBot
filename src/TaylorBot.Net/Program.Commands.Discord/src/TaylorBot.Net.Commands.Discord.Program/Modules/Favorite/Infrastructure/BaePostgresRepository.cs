using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class BaePostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IBaeRepository
{
    public ValueTask<string?> GetBaeAsync(IUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "bae");

    public ValueTask SetBaeAsync(IUser user, string songs) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "bae", songs);

    public ValueTask ClearBaeAsync(IUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "bae");
}
