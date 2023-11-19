using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class ObsessionPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IObsessionRepository
{
    public ValueTask<string?> GetObsessionAsync(IUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "waifu");

    public ValueTask SetObsessionAsync(IUser user, string songs) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "waifu", songs);

    public ValueTask ClearObsessionAsync(IUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "waifu");
}
