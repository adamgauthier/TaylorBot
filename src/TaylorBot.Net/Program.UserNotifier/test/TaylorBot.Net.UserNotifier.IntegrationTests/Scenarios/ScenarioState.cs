using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed record MemberCounts(int Messages, int Words, int Minutes, long Experience, DateTime? LastSpokeAt, bool Alive);

public sealed partial class ScenarioState(ScenarioDatabase database)
{
    public async Task<MemberCounts> MemberAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<MemberCounts>(
            """
            SELECT message_count AS Messages, word_count AS Words, minute_count AS Minutes, experience AS Experience,
                last_spoke_at AS LastSpokeAt, alive AS Alive
            FROM guilds.guild_members WHERE guild_id = @GuildId AND user_id = @Id;
            """, new { GuildId = guild.Id, user.Id });
    }

    public async Task<long> ChannelMessagesAsync(ScenarioGuild guild)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<long>("SELECT message_count FROM guilds.text_channels WHERE channel_id = @ChannelId;", new { guild.ChannelId });
    }

    public async Task<long> TaypointsAsync(ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<long>("SELECT taypoint_count FROM users.users WHERE user_id = @Id;", new { user.Id });
    }

    public async Task<bool> ReminderExistsAsync(Guid reminder)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<bool>("SELECT EXISTS(SELECT FROM users.reminders WHERE reminder_id = @reminder);", new { reminder });
    }

    public async Task<long> QueuedMessagesAsync(ScenarioGuild guild, ScenarioUser user) =>
        (long)(await database.Redis.HashGetAsync("member-message-count-increments", $"guild:{guild.Id}:user:{user.Id}"));
}
