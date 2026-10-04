using Dapper;
using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    public async Task<string?> FeedCheckpointAsync(ScenarioGuild guild, UserNotifierJob job)
    {
        var (table, _, checkpoint) = FeedTables.For(job);
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<string?>($"SELECT {checkpoint} FROM {table} WHERE guild_id = @Id AND channel_id = @ChannelId;", new { guild.Id, guild.ChannelId });
    }
}
