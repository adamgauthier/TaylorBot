using Discord;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;

public record PlusUser(bool IsActive, int MaxPlusGuilds, IReadOnlyCollection<string> ActivePlusGuilds);

public interface IPlusUserRepository
{
    ValueTask<PlusUser?> GetPlusUserAsync(IUser user);
    ValueTask AddPlusGuildAsync(IUser user, IGuild guild);
    ValueTask DisablePlusGuildAsync(IUser user, IGuild guild);
}
