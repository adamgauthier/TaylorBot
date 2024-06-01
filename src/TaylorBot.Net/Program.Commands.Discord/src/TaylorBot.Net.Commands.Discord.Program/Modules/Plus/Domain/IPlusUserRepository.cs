using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;

public record PlusUser(bool IsActive, int MaxPlusGuilds, IReadOnlyCollection<string> ActivePlusGuilds);

public interface IPlusUserRepository
{
    ValueTask<PlusUser?> GetPlusUserAsync(DiscordUser user);
    ValueTask AddPlusGuildAsync(DiscordUser user, CommandGuild guild);
    ValueTask DisablePlusGuildAsync(DiscordUser user, CommandGuild guild);
}
