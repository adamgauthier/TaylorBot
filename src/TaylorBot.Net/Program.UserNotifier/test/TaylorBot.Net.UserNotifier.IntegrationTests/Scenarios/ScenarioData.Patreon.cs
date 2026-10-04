using Dapper;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task PatronAsync(ScenarioUser user, ScenarioGuild? guild = null)
    {
        await using var connection = database.CreateConnection();
        await connection.ExecuteAsync(
            "INSERT INTO plus.plus_users (user_id, active, max_plus_guilds, source) VALUES (@Id, true, 2, 'patreon');", new { user.Id });

        if (guild != null)
        {
            await connection.ExecuteAsync(
                "INSERT INTO plus.plus_guilds (guild_id, plus_user_id, state) VALUES (@GuildId, @Id, 'enabled');", new { GuildId = guild.Id, user.Id });
        }
    }
}
