using Dapper;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Infrastructure;

public class ValentinesPostgresRepository(PostgresConnectionFactory postgresConnectionFactory) : IValentinesRepository
{
    private sealed record ConfigDto(string config_key, string config_value);

    public async ValueTask<ValentinesConfig> GetConfigurationAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var configs = (await connection.QueryAsync<ConfigDto>(
            "SELECT config_key, config_value FROM valentines2025.config;")).ToList();

        string GetConfigValue(string key) =>
            configs.Single(c => c.config_key == key).config_value;

        return new(
            SpreadLoveRoleId: new(GetConfigValue("spread_love_role_id")),
            IncubationPeriod: TimeSpan.Parse(GetConfigValue("incubation_period")),
            BypassSpreadLimitRoleIds: [.. GetConfigValue("bypass_spread_limit_role_ids").Split(',').Select(i => new SnowflakeId(i))],
            SpreadLimit: int.Parse(GetConfigValue("spread_limit")),
            LoungeChannelId: new(GetConfigValue("lounge_channel_id")),
            GiveawaysEndTime: DateTimeOffset.Parse(GetConfigValue("giveaways_end_time")),
            TimeSpanBetweenGiveaways: TimeSpan.Parse(GetConfigValue("timespan_between_giveaways")),
            GiveawayTaypointPrizeMin: int.Parse(GetConfigValue("giveaway_prize_min")),
            GiveawayTaypointPrizeMax: int.Parse(GetConfigValue("giveaway_prize_max"))
        );
    }

    private sealed record RoleObtainedDto(string user_id, string username, string acquired_from_user_id, string acquired_from_username, DateTime acquired_at);

    public async ValueTask<RoleObtained?> GetRoleObtainedByUserAsync(DiscordMember user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var obtainedByUser = await connection.QuerySingleOrDefaultAsync<RoleObtainedDto?>(
            """
            SELECT user_id, username, acquired_from_user_id, acquired_from_username, acquired_at
            FROM valentines2025.role_obtained WHERE user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.User.Id}",
            }
        );

        return obtainedByUser == null ? null : new RoleObtained(
            new(obtainedByUser.acquired_from_user_id),
            obtainedByUser.acquired_from_username,
            new(obtainedByUser.user_id),
            obtainedByUser.username,
            obtainedByUser.acquired_at
        );
    }

    public async ValueTask<DateTimeOffset> SpreadRoleAsync(DiscordMember fromUser, DiscordMember toUser)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var acquiredAt = await connection.QuerySingleAsync<DateTime>(
            """
            INSERT INTO valentines2025.role_obtained (user_id, username, acquired_from_user_id, acquired_from_username)
            VALUES (@ToUserId, @ToUserUsername, @FromUserId, @FromUserUsername)
            RETURNING acquired_at;
            """,
            new
            {
                ToUserId = $"{toUser.User.Id}",
                ToUserUsername = toUser.User.Username,
                FromUserId = $"{fromUser.User.Id}",
                FromUserUsername = fromUser.User.Username,
            }
        );

        return acquiredAt;
    }

    public async ValueTask<IReadOnlyList<RoleObtained>> GetRoleObtainedFromUserAsync(DiscordUser user)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var records = await connection.QueryAsync<RoleObtainedDto>(
            """
            SELECT user_id, username, acquired_from_user_id, acquired_from_username, acquired_at
            FROM valentines2025.role_obtained WHERE acquired_from_user_id = @UserId;
            """,
            new
            {
                UserId = $"{user.Id}",
            }
        );

        return [.. records.Select(r => new RoleObtained(
            new(r.acquired_from_user_id),
            r.acquired_from_username,
            new(r.user_id),
            r.username,
            r.acquired_at
        ))];
    }

    public async ValueTask<IReadOnlyList<RoleObtained>> GetAllAsync()
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var records = await connection.QueryAsync<RoleObtainedDto>(
            """
            SELECT user_id, username, acquired_from_user_id, acquired_from_username, acquired_at
            FROM valentines2025.role_obtained;
            """
        );

        return [.. records.Select(r => new RoleObtained(
            new(r.acquired_from_user_id),
            r.acquired_from_username,
            new(r.user_id),
            r.username,
            r.acquired_at
        ))];
    }

    public async ValueTask<IReadOnlyList<RoleObtained>> GetAllReadyAsync(ValentinesConfig config)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();

        var records = await connection.QueryAsync<RoleObtainedDto>(
            """
            SELECT user_id, username, acquired_from_user_id, acquired_from_username, acquired_at
            FROM valentines2025.role_obtained AS acquired
            WHERE
                (SELECT COUNT(*) FROM valentines2025.role_obtained AS givenTo WHERE givenTo.acquired_from_user_id = acquired.user_id) < @SpreadLimit
            AND
                acquired_at + @IncubationPeriod < CURRENT_TIMESTAMP;
            """,
            new
            {
                config.SpreadLimit,
                config.IncubationPeriod,
            }
        );

        return [.. records.Select(r => new RoleObtained(
            new(r.acquired_from_user_id),
            r.acquired_from_username,
            new(r.user_id),
            r.username,
            r.acquired_at
        ))];
    }
}
