using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;
using TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Infrastructure;

public class GenderPostgresRepository : IGenderRepository
{
    private readonly TextAttributePostgresRepository _textAttributePostgresRepository;

    public GenderPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository)
    {
        _textAttributePostgresRepository = textAttributePostgresRepository;
    }

    public ValueTask<string?> GetGenderAsync(IUser user) =>
        _textAttributePostgresRepository.GetAttributeAsync(user, "gender");

    public ValueTask SetGenderAsync(IUser user, string gender) =>
        _textAttributePostgresRepository.SetAttributeAsync(user, "gender", gender);

    public ValueTask ClearGenderAsync(IUser user) =>
        _textAttributePostgresRepository.ClearAttributeAsync(user, "gender");
}
