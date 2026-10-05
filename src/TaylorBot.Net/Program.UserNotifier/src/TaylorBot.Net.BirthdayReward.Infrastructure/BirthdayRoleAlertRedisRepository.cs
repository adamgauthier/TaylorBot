using StackExchange.Redis;
using TaylorBot.Net.BirthdayReward.Domain;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Infrastructure;

public class BirthdayRoleAlertRedisRepository(ConnectionMultiplexer connection) : IBirthdayRoleAlertRepository
{
    private static readonly TimeSpan DeliveryReservation = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan NotificationInterval = TimeSpan.FromHours(24);

    private static string Key(SnowflakeId guildId, BirthdayRoleOperation operation) =>
        $"birthday-role-alert:guild:{guildId}:{operation}";

    public Task<bool> TryReserveAsync(SnowflakeId guildId, BirthdayRoleOperation operation, string token) =>
        connection.GetDatabase().LockTakeAsync(Key(guildId, operation), token, DeliveryReservation);

    public async Task MarkDeliveredAsync(SnowflakeId guildId, BirthdayRoleOperation operation, string token)
    {
        if (!await connection.GetDatabase().LockExtendAsync(Key(guildId, operation), token, NotificationInterval))
        {
            throw new InvalidOperationException($"Birthday role alert reservation expired or changed for guild {guildId}, operation {operation}.");
        }
    }
}
