using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using TaylorBot.Net.Core.Infrastructure;
using TaylorBot.Net.Core.Infrastructure.Taypoints;
using TaylorBot.Net.PatreonSync.Domain;

namespace TaylorBot.Net.PatreonSync.Infrastructure;

public partial class PlusPostgresRepository(ILogger<PlusPostgresRepository> logger, PostgresConnectionFactory postgresConnectionFactory) : IPlusRepository
{
    private sealed record PlusUserDto(string? rewarded_for_charge_at, int max_plus_guilds, string source);

    private sealed class PlusGuildDto
    {
        public string guild_name { get; set; } = null!;
        public string state { get; set; } = null!;
    }

    private sealed class RewardedUserDto
    {
        public long taypoint_count { get; set; }
    }

    public async ValueTask<IUpdatePlusUserResult> AddOrUpdatePlusUserAsync(Patron patron)
    {
        await using var connection = postgresConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        var result = await UpsertPlusUserAsync(connection, patron);

        await transaction.CommitAsync();
        return result;
    }

    private async ValueTask<IUpdatePlusUserResult> UpsertPlusUserAsync(NpgsqlConnection connection, Patron patron)
    {
        var logPrefix = $"Upserting patron {patron.DiscordUserId}:";

        var userId = patron.DiscordUserId.ToString();
        var maxPlusGuilds = patron.CurrentlyEntitledAmountCents / 100L;

        var existingPlusUser = await connection.QuerySingleOrDefaultAsync<PlusUserDto?>(
            """
            SELECT rewarded_for_charge_at, max_plus_guilds, source FROM plus.plus_users
            WHERE user_id = @UserId FOR UPDATE;
            """,
            new
            {
                UserId = userId,
            }
        );

        if (existingPlusUser != null)
        {
            var plusGuilds = (await connection.QueryAsync<PlusGuildDto>(
                """
                SELECT guild_name, state
                FROM plus.plus_guilds INNER JOIN guilds.guilds ON plus.plus_guilds.guild_id = guilds.guilds.guild_id
                WHERE plus_user_id = @UserId;
                """,
                new
                {
                    UserId = userId,
                }
            )).ToList();

            var rewardedForChargeAtUpdate =
                patron.LastCharge != null && patron.LastCharge.Status == PatreonChargeStatus.Paid &&
                patron.LastCharge.Date != existingPlusUser.rewarded_for_charge_at ?
                patron.LastCharge.Date : null;

            LogPlusUserExists(logPrefix, patron.IsActive, maxPlusGuilds, rewardedForChargeAtUpdate);

            await connection.ExecuteAsync(
                """
                UPDATE plus.plus_users SET
                    active = @Active,
                    max_plus_guilds = @MaxPlusGuilds,
                    source = 'patreon',
                    rewarded_for_charge_at = (CASE
                        WHEN @RewardedForChargeAt IS NULL
                        THEN plus.plus_users.rewarded_for_charge_at
                        ELSE @RewardedForChargeAt
                    END),
                    metadata = @Metadata
                WHERE user_id = @UserId;
                """,
                new
                {
                    UserId = userId,
                    Active = patron.IsActive,
                    MaxPlusGuilds = maxPlusGuilds,
                    RewardedForChargeAt = rewardedForChargeAtUpdate,
                    Metadata = patron.Metadata,
                }
            );

            if (rewardedForChargeAtUpdate != null)
            {
                var rewardAmount = patron.CurrentlyEntitledAmountCents * 10;
                LogRewardingPoints(logPrefix, rewardAmount, existingPlusUser.rewarded_for_charge_at);

                var rewardedUser = await TaypointPostgresUtil.AddTaypointsReturningAsync(connection, userId, rewardAmount);

                return new UserRewarded(Reward: rewardAmount, NewTaypointCount: rewardedUser.taypoint_count);
            }
            else if (patron.IsActive)
            {
                if (plusGuilds.Count(g => g.state == "enabled") > maxPlusGuilds)
                {
                    var enabledPlusGuilds = plusGuilds.Where(g => g.state == "enabled").Select(g => g.guild_name).ToList();
                    LogDisablingGuildsForLoweredPledge(logPrefix, enabledPlusGuilds.Count);

                    await connection.ExecuteAsync(
                        """
                        UPDATE plus.plus_guilds SET state = 'auto_disabled'
                        WHERE plus_user_id = @UserId AND state = 'enabled';
                        """,
                        new
                        {
                            UserId = userId,
                        }
                    );

                    return new GuildsDisabledForLoweredPledge(enabledPlusGuilds, maxPlusGuilds);
                }
                else if (plusGuilds.Any(g => g.state == "auto_disabled") &&
                         plusGuilds.Count(g => g.state is "enabled" or "auto_disabled") <= maxPlusGuilds)
                {
                    LogEnablingAutoDisabledGuilds(logPrefix, plusGuilds.Count(g => g.state == "auto_disabled"));

                    await connection.ExecuteAsync(
                        """
                        UPDATE plus.plus_guilds SET state = 'enabled'
                        WHERE plus_user_id = @UserId AND state = 'auto_disabled';
                        """,
                        new
                        {
                            UserId = userId,
                        }
                    );

                    return new UserUpdated();
                }
                else
                {
                    return new UserUpdated();
                }
            }
            else if (plusGuilds.Any(g => g.state == "enabled"))
            {
                var enabledPlusGuilds = plusGuilds.Where(g => g.state == "enabled").Select(g => g.guild_name).ToList();
                LogDisablingGuildsForInactivity(logPrefix, enabledPlusGuilds.Count);

                await connection.ExecuteAsync(
                    """
                    UPDATE plus.plus_guilds SET state = 'auto_disabled'
                    WHERE plus_user_id = @UserId AND state = 'enabled';
                    """,
                    new
                    {
                        UserId = userId,
                    }
                );

                return new GuildsDisabledForInactivity(enabledPlusGuilds);
            }
            else
            {
                return new UserUpdated();
            }
        }
        else
        {
            LogPlusUserDoesntExist(logPrefix, patron.IsActive, maxPlusGuilds);

            await connection.ExecuteAsync(
                """
                INSERT INTO plus.plus_users (user_id, active, max_plus_guilds, source, rewarded_for_charge_at, metadata)
                VALUES (@UserId, @Active, @MaxPlusGuilds, 'patreon', NULL, @Metadata);
                """,
                new
                {
                    UserId = userId,
                    Active = patron.IsActive,
                    MaxPlusGuilds = maxPlusGuilds,
                    Metadata = patron.Metadata,
                }
            );

            return patron.IsActive ? new ActiveUserAdded() : new InactiveUserAdded();
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Prefix} Plus user already exists, updating with IsActive={IsActive}, MaxPlusGuilds={MaxPlusGuilds}, RewardedForChargeAtUpdate={RewardedForChargeAtUpdate}.")]
    private partial void LogPlusUserExists(string prefix, bool isActive, long maxPlusGuilds, string? rewardedForChargeAtUpdate);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Prefix} Rewarding {RewardAmount} points because rewarded_for_charge_at={RewardedForChargeAt}.")]
    private partial void LogRewardingPoints(string prefix, long rewardAmount, string? rewardedForChargeAt);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Prefix} Disabling plus guilds because enabled count is {EnabledPlusGuildsCount}.")]
    private partial void LogDisablingGuildsForLoweredPledge(string prefix, int enabledPlusGuildsCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Prefix} Enabling {AutoDisabledGuildCount} auto disabled plus guilds.")]
    private partial void LogEnablingAutoDisabledGuilds(string prefix, int autoDisabledGuildCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Prefix} Disabling {EnabledPlusGuildsCount} plus guilds because patron isn't active.")]
    private partial void LogDisablingGuildsForInactivity(string prefix, int enabledPlusGuildsCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Prefix} Plus user doesn't exist, adding with IsActive={IsActive}, MaxPlusGuilds={MaxPlusGuilds}.")]
    private partial void LogPlusUserDoesntExist(string prefix, bool isActive, long maxPlusGuilds);
}
