using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class FavoriteSongsPostgresRepository : IFavoriteSongsRepository
{
    private readonly TextAttributePostgresRepository _textAttributePostgresRepository;

    public FavoriteSongsPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository)
    {
        _textAttributePostgresRepository = textAttributePostgresRepository;
    }

    public ValueTask<string?> GetFavoriteSongsAsync(IUser user) =>
        _textAttributePostgresRepository.GetAttributeAsync(user, "favoritesongs");

    public ValueTask SetFavoriteSongsAsync(IUser user, string songs) =>
        _textAttributePostgresRepository.SetAttributeAsync(user, "favoritesongs", songs);

    public ValueTask ClearFavoriteSongsAsync(IUser user) =>
        _textAttributePostgresRepository.ClearAttributeAsync(user, "favoritesongs");
}
