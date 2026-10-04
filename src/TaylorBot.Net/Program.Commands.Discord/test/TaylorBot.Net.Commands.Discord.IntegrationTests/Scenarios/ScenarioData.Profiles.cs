using Dapper;
using System.Net;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task BirthdayAsync(ScenarioUser user, DateOnly birthday, bool isPrivate = false, DateTime? lastRewardAt = null)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO attributes.birthdays (user_id, birthday, is_private, last_reward_at)
            VALUES (@Id, @birthday, @isPrivate, @lastRewardAt)
            ON CONFLICT (user_id) DO UPDATE SET birthday = excluded.birthday, is_private = excluded.is_private,
                last_reward_at = excluded.last_reward_at;
            """, new { user.Id, birthday, isPrivate, lastRewardAt });
    }

    public async Task BirthdayCalendarAsync(ScenarioGuild guild, int count)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        for (var index = 0; index < count; index++)
        {
            var user = await UserAsync(username: $"BirthdayMember{index:D2}");
            await MemberAsync(guild, user);
            await BirthdayAsync(user, date.AddDays(index).AddYears(-25));
        }

        await RefreshBirthdayCalendarAsync();
    }

    public async Task RefreshBirthdayCalendarAsync()
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("REFRESH MATERIALIZED VIEW attributes.birthday_calendar_6months;");
    }

    public async Task BirthdayRoleAsync(ScenarioGuild guild, ScenarioRole role)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO plus.birthday_roles (guild_id, role_id) VALUES (@GuildId, @RoleId);",
            new { GuildId = guild.Id, RoleId = role.Id });
    }

    public void BirthdayRoleCreation(ScenarioGuild guild, ScenarioRole role) =>
        _api.ExpectRequest("POST", $"guilds/{guild.Id}/roles", role.Payload(), HttpStatusCode.OK);

    public void BirthdayRoleDeletion(ScenarioGuild guild, ScenarioRole role) =>
        _api.ExpectRequest("DELETE", $"guilds/{guild.Id}/roles/{role.Id}");

    public async Task AgeRoleAsync(ScenarioGuild guild, ScenarioRole role, int minimumAge)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("INSERT INTO plus.age_roles (guild_id, age_role_id, minimum_age) VALUES (@GuildId, @RoleId, @minimumAge);",
            new { GuildId = guild.Id, RoleId = role.Id, minimumAge });
    }

    public async Task BirthdayRoleAwardAsync(ScenarioGuild guild, ScenarioUser user, ScenarioRole role)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO plus.birthday_roles_given (guild_id, user_id, role_id, set_at, remove_at)
            VALUES (@GuildId, @Id, @RoleId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + interval '40 hours');
            """, new { GuildId = guild.Id, user.Id, RoleId = role.Id });
    }

    public async Task ReminderAsync(ScenarioUser user, string text = "Listen to the album")
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.reminders (user_id, remind_at, reminder_text)
            VALUES (@Id, CURRENT_TIMESTAMP + interval '1 hour', @text);
            """, new { user.Id, text });
    }
}
