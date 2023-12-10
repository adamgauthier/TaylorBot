using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;

namespace TaylorBot.Net.Commands.Infrastructure;

public class CommandPrefixPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : ICommandPrefixRepository
{
    private record PrefixDto(string prefix, bool was_inserted, bool guild_name_changed, string? previous_guild_name);

    public async ValueTask<CommandPrefix> GetOrInsertGuildPrefixAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var dto = await connection.QuerySingleAsync<PrefixDto>(
            """
            INSERT INTO guilds.guilds (guild_id, guild_name, previous_guild_name) VALUES (@GuildId, @GuildName, NULL)
            ON CONFLICT (guild_id) DO UPDATE SET
                previous_guild_name = guilds.guilds.guild_name,
                guild_name = excluded.guild_name
            RETURNING prefix, previous_guild_name IS NULL AS was_inserted, previous_guild_name IS DISTINCT FROM guild_name AS guild_name_changed, previous_guild_name;
            """,
            new
            {
                GuildId = $"{guild.Id}",
                GuildName = guild.Name,
            }
        );

        return new CommandPrefix(new(dto.was_inserted, dto.guild_name_changed, dto.previous_guild_name), dto.prefix);
    }

    public async ValueTask ChangeGuildPrefixAsync(IGuild guild, string prefix)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "UPDATE guilds.guilds SET prefix = @Prefix WHERE guild_id = @GuildId;",
            new
            {
                Prefix = prefix,
                GuildId = $"{guild.Id}",
            }
        );
    }
}
