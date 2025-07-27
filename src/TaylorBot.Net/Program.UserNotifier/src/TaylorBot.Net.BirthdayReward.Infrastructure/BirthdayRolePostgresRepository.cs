using Dapper;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Infrastructure;

public class BirthdayRolePostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IBirthdayRoleRepository
{
    public async Task<List<BirthdayUser>> GetBirthdayUsersAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return [.. (await connection.QueryAsync<BirthdayUser>(
            """
            SELECT user_id, birthday_end FROM (
            	SELECT
            		user_id,
            		(birthday_date - INTERVAL '8 HOURS') AT TIME ZONE 'UTC' AS birthday_start,
            		(birthday_date + INTERVAL '32 HOURS') AT TIME ZONE 'UTC' AS birthday_end
            	FROM (
            		SELECT user_id, (birthday + (INTERVAL '1 YEAR' * (date_part('year', CURRENT_DATE) - date_part('year', birthday))))::date as birthday_date
            		FROM attributes.birthdays
                    WHERE is_private IS FALSE AND birthday != '-infinity'
            	) AS birthdays
            ) AS birthday_windows
            WHERE CURRENT_TIMESTAMP AT TIME ZONE 'UTC' BETWEEN birthday_start AND birthday_end;
            """
        ))];
    }

    public async Task<List<BirthdayRole>> GetRolesAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return [.. (await connection.QueryAsync<BirthdayRole>(
            """
            SELECT guild_id, role_id
            FROM plus.birthday_roles;
            """
        ))];
    }

    public async Task<List<SnowflakeId>> GetGuildsForUserAsync(SnowflakeId userId, IReadOnlyList<BirthdayRole> roles)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return [.. (await connection.QueryAsync<string>(
            """
            SELECT guild_id
            FROM guilds.guild_members
            WHERE user_id = @UserId
            AND guild_id = ANY(@GuildIds)
            AND alive IS TRUE;
            """,
            new
            {
                UserId = $"{userId}",
                GuildIds = roles.Select(r => r.guild_id).ToList(),
            }
        )).Select(g => new SnowflakeId(g))];
    }

    public async Task CreateRoleGivenAsync(BirthdayUser birthdayUser, BirthdayRole role, DateTimeOffset setAt)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO plus.birthday_roles_given (guild_id, user_id, role_id, set_at, remove_at, removed_at)
            VALUES (@GuildId, @UserId, @RoleId, @SetAt, @RemoveAt, NULL)
            ON CONFLICT (guild_id, user_id) DO UPDATE SET
                role_id = excluded.role_id,
                set_at = excluded.set_at,
                remove_at = excluded.remove_at,
                removed_at = excluded.removed_at;
            """,
            new
            {
                GuildId = role.guild_id,
                UserId = birthdayUser.user_id,
                RoleId = role.role_id,
                SetAt = setAt,
                RemoveAt = birthdayUser.birthday_end,
            }
        );
    }

    public async Task<List<BirthdayRoleToRemove>> GetRolesToRemoveAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return [.. (await connection.QueryAsync<BirthdayRoleToRemove>(
            """
            SELECT guild_id, user_id, role_id, set_at
            FROM plus.birthday_roles_given
            WHERE removed_at IS NULL AND remove_at <= CURRENT_TIMESTAMP;
            """
        ))];
    }

    public async Task SetRoleRemovedAtAsync(BirthdayRoleToRemove toRemove)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            """
            UPDATE plus.birthday_roles_given
            SET removed_at = NOW()
            WHERE guild_id = @GuildId AND user_id = @UserId;
            """,
            new
            {
                GuildId = toRemove.guild_id,
                UserId = toRemove.user_id,
            }
        );
    }

    public async Task<DateTime?> GetLastTimeRoleWasGivenAsync(BirthdayUser user, SnowflakeId guildId)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<DateTime?>(
            """
            SELECT set_at
            FROM plus.birthday_roles_given
            WHERE guild_id = @GuildId AND user_id = @UserId;
            """,
            new
            {
                GuildId = $"{guildId}",
                UserId = user.user_id,
            }
        );
    }
}
