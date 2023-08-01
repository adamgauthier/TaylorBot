using Discord;
using System.Threading.Tasks;

namespace TaylorBot.Net.EntityTracker.Domain.Guild;

public record GuildAddedResult(bool WasAdded, bool WasGuildNameChanged, string? PreviousGuildName);

public interface IGuildRepository
{
    ValueTask<GuildAddedResult> AddGuildIfNotAddedAsync(IGuild guild);
}
