using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class BaePostgresRepository : IBaeRepository
{
    private readonly TextAttributePostgresRepository _textAttributePostgresRepository;

    public BaePostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository)
    {
        _textAttributePostgresRepository = textAttributePostgresRepository;
    }

    public ValueTask<string?> GetBaeAsync(IUser user) =>
        _textAttributePostgresRepository.GetAttributeAsync(user, "bae");

    public ValueTask SetBaeAsync(IUser user, string songs) =>
        _textAttributePostgresRepository.SetAttributeAsync(user, "bae", songs);

    public ValueTask ClearBaeAsync(IUser user) =>
        _textAttributePostgresRepository.ClearAttributeAsync(user, "bae");
}
