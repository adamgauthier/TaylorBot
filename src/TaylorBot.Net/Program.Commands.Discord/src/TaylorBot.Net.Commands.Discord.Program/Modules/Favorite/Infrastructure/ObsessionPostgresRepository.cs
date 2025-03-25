using TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Infrastructure;

public class ObsessionPostgresRepository(TextAttributePostgresRepository textAttributePostgresRepository) : IObsessionRepository
{
    public ValueTask<string?> GetObsessionAsync(DiscordUser user) =>
        textAttributePostgresRepository.GetAttributeAsync(user, "waifu");

    public ValueTask SetObsessionAsync(DiscordUser user, string obsession) =>
        textAttributePostgresRepository.SetAttributeAsync(user, "waifu", obsession);

    public ValueTask ClearObsessionAsync(DiscordUser user) =>
        textAttributePostgresRepository.ClearAttributeAsync(user, "waifu");
}
