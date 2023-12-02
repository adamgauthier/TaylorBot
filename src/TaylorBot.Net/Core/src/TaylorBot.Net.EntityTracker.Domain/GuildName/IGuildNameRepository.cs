using Discord;

namespace TaylorBot.Net.EntityTracker.Domain.GuildName;

public interface IGuildNameRepository
{
    ValueTask AddNewGuildNameAsync(IGuild guild);
}
