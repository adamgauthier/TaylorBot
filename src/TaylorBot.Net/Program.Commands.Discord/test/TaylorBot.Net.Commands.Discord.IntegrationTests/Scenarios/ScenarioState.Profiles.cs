using Dapper;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed record BirthdayState(DateOnly Birthday, bool IsPrivate, DateTime? LastRewardAt);
public sealed record LocationState(string Address, string Latitude, string Longitude, string TimeZone);
public sealed record ReminderState(string Text, DateTime RemindAt);

public sealed partial class ScenarioState
{
    public Task<string?> ProfileTextAsync(ScenarioUser user, string attribute) =>
        ReadAsync<string?>("SELECT attribute_value FROM attributes.text_attributes WHERE user_id = @Id AND attribute_id = @attribute;",
            new { user.Id, attribute });

    public Task<BirthdayState?> BirthdayAsync(ScenarioUser user) =>
        ReadAsync<BirthdayState>("SELECT birthday AS Birthday, is_private AS IsPrivate, last_reward_at AS LastRewardAt FROM attributes.birthdays WHERE user_id = @Id;",
            new { user.Id });

    public Task<string?> BirthdayRoleAsync(ScenarioGuild guild) =>
        ReadAsync<string?>("SELECT role_id FROM plus.birthday_roles WHERE guild_id = @Id;", new { guild.Id });

    public Task<long> BirthdayRoleAwardsAsync(ScenarioGuild guild) =>
        ReadAsync<long>("SELECT COUNT(*) FROM plus.birthday_roles_given WHERE guild_id = @Id;", new { guild.Id });

    public Task<LocationState?> LocationAsync(ScenarioUser user) =>
        ReadAsync<LocationState>("SELECT formatted_address AS Address, latitude AS Latitude, longitude AS Longitude, timezone_id AS TimeZone FROM attributes.location_attributes WHERE user_id = @Id;",
            new { user.Id });

    public async Task<IReadOnlyList<ReminderState>> RemindersAsync(ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        return [.. await connection.QueryAsync<ReminderState>(
            "SELECT reminder_text AS Text, remind_at AS RemindAt FROM users.reminders WHERE user_id = @Id ORDER BY remind_at, reminder_text;",
            new { user.Id })];
    }
}
