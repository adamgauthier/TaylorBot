using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    public async Task<bool> BirthdayRewardRecordedAsync(ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<bool>("SELECT last_reward_at IS NOT NULL FROM attributes.birthdays WHERE user_id = @Id;", new { user.Id });
    }

    public async Task<IReadOnlyList<string>> CalendarUsersAsync()
    {
        await using var connection = database.CreateConnection();
        return [.. await connection.QueryAsync<string>("SELECT user_id FROM attributes.birthday_calendar_6months;")];
    }

    public async Task<bool> BirthdayRoleRecordedAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<bool>(
            "SELECT EXISTS(SELECT FROM plus.birthday_roles_given WHERE guild_id = @GuildId AND user_id = @Id);", new { GuildId = guild.Id, user.Id });
    }

    public async Task<bool> BirthdayRoleRemovedAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<bool>(
            "SELECT removed_at IS NOT NULL FROM plus.birthday_roles_given WHERE guild_id = @GuildId AND user_id = @Id;", new { GuildId = guild.Id, user.Id });
    }
}
