using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Domain;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Infrastructure;

public class GuildNamesPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IGuildNamesRepository
{
    private record GuildNameDto(string guild_name, DateTime changed_at);

    public async ValueTask<List<GuildNameEntry>> GetHistoryAsync(IGuild guild, int limit)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var serverNames = await connection.QueryAsync<GuildNameDto>(
            """
            SELECT guild_name, changed_at
            FROM guilds.guild_names
            WHERE guild_id = @GuildId
            ORDER BY changed_at DESC
            LIMIT @MaxRows;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                MaxRows = limit,
            }
        );

        return serverNames.Select(n => new GuildNameEntry(n.guild_name, n.changed_at)).ToList();
    }
}
