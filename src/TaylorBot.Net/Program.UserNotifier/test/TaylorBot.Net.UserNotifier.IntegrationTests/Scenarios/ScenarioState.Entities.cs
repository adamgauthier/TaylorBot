using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    public async Task<IReadOnlyList<string>> UsernamesAsync(ScenarioUser user)
    {
        await using var connection = database.CreateConnection();
        return [.. await connection.QueryAsync<string>("SELECT username FROM users.usernames WHERE user_id = @Id;", new { user.Id })];
    }

    public async Task<IReadOnlyList<string>> GuildNamesAsync(ScenarioGuild guild)
    {
        await using var connection = database.CreateConnection();
        return [.. await connection.QueryAsync<string>("SELECT guild_name FROM guilds.guild_names WHERE guild_id = @Id;", new { guild.Id })];
    }

    public async Task<bool> ChannelExistsAsync(string channelId)
    {
        await using var connection = database.CreateConnection();
        return await connection.QuerySingleAsync<bool>("SELECT EXISTS(SELECT FROM guilds.text_channels WHERE channel_id = @channelId);", new { channelId });
    }
}
