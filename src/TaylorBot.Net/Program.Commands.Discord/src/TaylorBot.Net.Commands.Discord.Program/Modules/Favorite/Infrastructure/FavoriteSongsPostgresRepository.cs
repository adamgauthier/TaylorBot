using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class FavoriteSongsPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IFavoriteSongsRepository
{
    public ValueTask<string?> GetFavoriteSongsAsync(DiscordUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "favoritesongs");

    public ValueTask SetFavoriteSongsAsync(DiscordUser user, string songs) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "favoritesongs", songs);

    public ValueTask ClearFavoriteSongsAsync(DiscordUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "favoritesongs");
}
