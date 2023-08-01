using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.Guild;

namespace TaylorBot.Net.EntityTracker.Infrastructure.Guild;

public class GuildRepository : IGuildRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public GuildRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    private record GuildAddedOrUpdatedDto(bool was_inserted, bool guild_name_changed, string? previous_guild_name);

    public async ValueTask<GuildAddedResult> AddGuildIfNotAddedAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

        var guildAddedOrUpdatedDto = await connection.QuerySingleAsync<GuildAddedOrUpdatedDto>(
            """
            INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES (@GuildId, @GuildName, NULL)
            ON CONFLICT (guild_id) DO UPDATE SET
                previous_guild_name = guilds.guilds.guild_name,
                guild_name = excluded.guild_name
            RETURNING previous_guild_name IS NULL AS was_inserted, previous_guild_name IS DISTINCT FROM guild_name AS guild_name_changed, previous_guild_name;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                GuildName = guild.Name,
            }
        );

        return new GuildAddedResult(
            WasAdded: guildAddedOrUpdatedDto.was_inserted,
            WasGuildNameChanged: guildAddedOrUpdatedDto.guild_name_changed,
            PreviousGuildName: guildAddedOrUpdatedDto.previous_guild_name
        );
    }
}
