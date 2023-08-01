using Dapper;
using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.EntityTracker.Domain.GuildName;

namespace TaylorBot.Net.EntityTracker.Infrastructure.GuildName;

public class GuildNameRepository : IGuildNameRepository
{
    private readonly PostgresConnectionFactory _postgresConnectionFactory;

    public GuildNameRepository(PostgresConnectionFactory postgresConnectionFactory)
    {
        _postgresConnectionFactory = postgresConnectionFactory;
    }

    public async ValueTask AddNewGuildNameAsync(IGuild guild)
    {
        await using var connection = _postgresConnectionFactory.CreateConnection();

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
