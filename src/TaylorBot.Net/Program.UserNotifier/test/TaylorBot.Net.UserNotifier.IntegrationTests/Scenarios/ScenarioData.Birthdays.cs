using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task BirthdayAsync(ScenarioUser user, int daysAgo = 0, bool isPrivate = false, bool rewarded = false)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO attributes.birthdays (user_id, birthday, is_private, last_reward_at)
            VALUES (@Id, (CURRENT_DATE - interval '25 years' - @daysAgo * interval '1 day')::date, @isPrivate,
                CASE WHEN @rewarded THEN CURRENT_TIMESTAMP ELSE NULL END);
            """, new { user.Id, daysAgo, isPrivate, rewarded });
    }

    public async Task<string> BirthdayRoleAsync(ScenarioGuild guild, bool exists = true)
    {
        var roleId = $"{Interlocked.Increment(ref _id)}";
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO plus.birthday_roles (guild_id, role_id) VALUES (@Id, @roleId);", new { guild.Id, roleId });

        if (exists)
        {
            await dispatch("GUILD_ROLE_CREATE", new
            {
                guild_id = guild.Id,
                role = new { id = roleId, name = "Birthday", color = 0, hoist = false, position = 1, permissions = "0", managed = false, mentionable = false, flags = 0 },
            });
        }

        return roleId;
    }

    public async Task BirthdayRoleGivenAsync(ScenarioGuild guild, ScenarioUser user, string roleId, bool expired = true, bool hasRole = true)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO plus.birthday_roles_given (guild_id, user_id, role_id, set_at, remove_at)
            VALUES (@GuildId, @Id, @roleId, CURRENT_TIMESTAMP - interval '2 days', CURRENT_TIMESTAMP + @remaining);
            """, new { GuildId = guild.Id, user.Id, roleId, remaining = expired ? -TimeSpan.FromMinutes(1) : TimeSpan.FromDays(1) });

        if (hasRole)
        {
            await dispatch("GUILD_MEMBER_UPDATE", new
            {
                guild_id = guild.Id,
                user = Discord.NotifierDiscordApi.User(user.Id, user.Username),
                roles = new[] { roleId },
                joined_at = "2026-01-01T00:00:00Z",
            });
        }
    }
}
