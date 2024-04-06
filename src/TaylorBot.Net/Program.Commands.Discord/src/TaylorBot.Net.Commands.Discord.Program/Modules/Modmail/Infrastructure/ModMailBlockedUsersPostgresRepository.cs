using Dapper;
using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Infrastructure;

public class ModMailBlockedUsersPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IModMailBlockedUsersRepository
{
    public async ValueTask<int> GetBlockedUserCountAsync(IGuild guild)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<int>(
            """
            SELECT COUNT(*) FROM moderation.mod_mail_blocked_users
            WHERE guild_id = @GuildId;
            """,
            new
            {
                GuildId = $"{guild.Id}",
            }
        );
    }

    public async ValueTask BlockAsync(IGuild guild, DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO moderation.mod_mail_blocked_users (guild_id, user_id)
            VALUES (@GuildId, @UserId)
            ON CONFLICT (guild_id, user_id) DO NOTHING;
            """,
            new
            {
                GuildId = guild.Id.ToString(),
                UserId = user.Id.ToString(),
            }
        );
    }

    public async ValueTask UnblockAsync(IGuild guild, DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "DELETE FROM moderation.mod_mail_blocked_users WHERE guild_id = @GuildId AND user_id = @UserId;",
            new
            {
                GuildId = $"{guild.Id}",
                UserId = $"{user.Id}",
            }
        );
    }

    public async ValueTask<bool> IsBlockedAsync(IGuild guild, DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<bool>(
            """
            SELECT EXISTS(
                SELECT FROM moderation.mod_mail_blocked_users WHERE guild_id = @GuildId AND user_id = @UserId
            );
            """,
            new
            {
                GuildId = $"{guild.Id}",
                UserId = $"{user.Id}",
            }
        );
    }
}
