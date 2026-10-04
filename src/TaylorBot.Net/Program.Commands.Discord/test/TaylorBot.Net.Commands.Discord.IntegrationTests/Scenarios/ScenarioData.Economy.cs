using System.Text.Json;
using Dapper;

namespace TaylorBot.Net.Commands.Discord.IntegrationTests.Scenarios;

public sealed partial class ScenarioData
{
    public async Task DailyRecordAsync(ScenarioUser user, int current = 2, int maximum = 10)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users.daily_payouts (user_id, streak_count, max_streak_count, last_payout_at)
            VALUES (@Id, @current, @maximum, CURRENT_TIMESTAMP)
            ON CONFLICT (user_id) DO UPDATE SET streak_count = @current, max_streak_count = @maximum;
            """, new { user.Id, current, maximum });
    }

    public async Task GameProfileAsync(ScenarioUser user, string game, int wins = 3, int draws = 2, int losses = 1)
    {
        var sql = game switch
        {
            "risk" => "INSERT INTO users.risk_stats (user_id, risk_win_count, risk_win_amount, risk_lose_count, risk_lose_amount) VALUES (@Id, @wins, 120, @losses, 20);",
            "heist" => "INSERT INTO users.heist_stats (user_id, heist_win_count, heist_win_amount, heist_lose_count, heist_lose_amount) VALUES (@Id, @wins, 120, @losses, 20);",
            "roll" => "INSERT INTO users.roll_stats (user_id, roll_count, perfect_roll_count) VALUES (@Id, 30, @wins);",
            "rps" => "INSERT INTO users.rps_stats (user_id, rps_win_count, rps_draw_count, rps_lose_count) VALUES (@Id, @wins, @draws, @losses);",
            _ => throw new ArgumentOutOfRangeException(nameof(game)),
        };
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync(sql, new { user.Id, wins, draws, losses });
    }

    public async Task EconomyLeaderboardAsync(ScenarioGuild guild, string feature)
    {
        for (var index = 1; index <= 16; index++)
        {
            var member = await UserAsync(taypoints: 1000 - index, username: $"Rank{index:D2}");
            await MemberAsync(guild, member);
            await using var connection = _database.CreateConnection();
            await connection.ExecuteAsync("""
                UPDATE guilds.guild_members SET last_known_taypoint_count = @Score,
                    message_count = @Score, minute_count = @Score, first_joined_at = @Joined
                WHERE guild_id = @GuildId AND user_id = @Id;
                """, new { GuildId = guild.Id, member.Id, Score = 1000 - index, Joined = new DateTime(year: 2020, month: 1, day: index, hour: 0, minute: 0, second: 0, DateTimeKind.Utc) });
            if (feature == "daily")
            {
                await DailyRecordAsync(member, current: 1000 - index, maximum: 1000 - index);
            }
            else if (feature is "risk" or "heist" or "roll" or "rps")
            {
                await GameProfileAsync(member, feature, wins: 1000 - index);
            }
        }
    }

    public async Task ServerActivityAsync(ScenarioGuild guild, ScenarioUser user, int messages = 12, int words = 30, int minutes = 90)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE guilds.guild_members SET message_count = @messages, word_count = @words,
                minute_count = @minutes, first_joined_at = '2020-01-02T00:00:00Z'
            WHERE guild_id = @GuildId AND user_id = @Id;
            """, new { GuildId = guild.Id, user.Id, messages, words, minutes });
    }

    public async Task MissingJoinedDateAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("UPDATE guilds.guild_members SET first_joined_at = NULL WHERE guild_id = @GuildId AND user_id = @Id;",
            new { GuildId = guild.Id, user.Id });
    }

    public async Task ServerNamesAsync(ScenarioGuild guild)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO guilds.guild_names (guild_id, guild_name, changed_at)
            SELECT @Id, 'Previous name ' || to_char(i, 'FM00'), '2020-01-01'::timestamptz + i * interval '1 day'
            FROM generate_series(1, 16) AS i;
            """, new { guild.Id });
    }

    public async Task ChannelMessagesAsync(ScenarioGuild guild, bool spam)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO guilds.text_channels (guild_id, channel_id, message_count, is_spam)
            VALUES (@Id, @ChannelId, 1234, @spam)
            ON CONFLICT (guild_id, channel_id) DO UPDATE SET message_count = 1234, is_spam = @spam;
            """, new { guild.Id, guild.ChannelId, spam });
    }

    public async Task HeistBankAsync(int minimumRoll)
    {
        var json = JsonSerializer.Serialize(new[] { new { bankName = "Integration Vault", maximumUserCount = (int?)null, minimumRollForSuccess = minimumRoll, payoutMultiplier = "2" } });
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO configuration.application_info (info_key, info_value) VALUES ('banks_json', @json)
            ON CONFLICT (info_key) DO UPDATE SET info_value = @json;
            """, new { json });
    }

    public async Task OpenHeistAsync(ScenarioGuild guild, ScenarioUser user, long investment = 10) =>
        await _database.Redis.HashSetAsync($"heist:guild:{guild.Id}", user.Id,
            JsonSerializer.Serialize(new { Absolute = new { Amount = investment, Balance = 100 }, Relative = (object?)null }));

    public async Task ExhaustGameLimitAsync(ScenarioUser user, string game) =>
        await _database.Redis.StringSetAsync($"user:{user.Id}:action:{game}:date:{DateTime.UtcNow:yyyy-MM-dd}", 10000);

    public async Task MemberLeftAsync(ScenarioGuild guild, ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("UPDATE guilds.guild_members SET alive = FALSE WHERE guild_id = @GuildId AND user_id = @Id;",
            new { GuildId = guild.Id, user.Id });
    }

    public async Task TaypointBalanceAsync(ScenarioUser user, long balance)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("UPDATE users.users SET taypoint_count = @balance WHERE user_id = @Id;", new { user.Id, balance });
    }

    public async Task ExcludedEconomyLeadersAsync(ScenarioGuild guild, string feature)
    {
        var outsider = await UserAsync(taypoints: 5000, username: "Outsider");
        var departed = await UserAsync(taypoints: 5000, username: "Departed");
        await MemberAsync(guild, departed);
        await MemberLeftAsync(guild, departed);
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE guilds.guild_members SET last_known_taypoint_count = 5000, message_count = 5000, minute_count = 5000
            WHERE guild_id = @GuildId AND user_id = @Id;
            """, new { GuildId = guild.Id, departed.Id });
        foreach (var user in new[] { outsider, departed })
        {
            if (feature == "daily")
            {
                await DailyRecordAsync(user, current: 5000, maximum: 5000);
            }
            else if (feature is "risk" or "heist" or "roll" or "rps")
            {
                await GameProfileAsync(user, feature, wins: 5000);
            }
        }
    }

    public async Task OldMinutesAsync(ScenarioUser user)
    {
        await using var connection = _database.CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO attributes.integer_attributes (attribute_id, user_id, integer_value) VALUES ('oldminutes', @Id, 123);
            """, new { user.Id });
    }
}
