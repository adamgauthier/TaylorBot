using Dapper;
using Discord;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.GuildName;

namespace TaylorBot.Net.EntityTracker.Infrastructure.GuildName;

public class GuildNameRepository(PostgresConnectionFactory postgresConnectionFactory) : IGuildNameRepository
{
    public async ValueTask AddNewGuildNameAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "INSERT INTO guilds.guild_names (guild_id, guild_name) VALUES (@GuildId, @GuildName);",
            new
            {
                GuildId = guild.Id.ToString(),
                GuildName = guild.Name
            }
        );
    }
}
