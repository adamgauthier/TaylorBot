namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Domain;

public record GuildNameEntry(string GuildName, DateTimeOffset ChangedAt);

public interface IGuildNamesRepository
{
    ValueTask<List<GuildNameEntry>> GetHistoryAsync(CommandGuild guild, int limit);
}
