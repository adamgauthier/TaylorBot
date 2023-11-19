using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class FavoriteSongsPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IFavoriteSongsRepository
{
    public ValueTask<string?> GetFavoriteSongsAsync(IUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "favoritesongs");

    public ValueTask SetFavoriteSongsAsync(IUser user, string songs) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "favoritesongs", songs);

    public ValueTask ClearFavoriteSongsAsync(IUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "favoritesongs");
}
