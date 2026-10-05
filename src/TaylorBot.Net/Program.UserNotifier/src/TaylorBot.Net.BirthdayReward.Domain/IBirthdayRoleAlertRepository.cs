using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.BirthdayReward.Domain;

public interface IBirthdayRoleAlertRepository
{
    Task<bool> TryReserveAsync(SnowflakeId guildId, BirthdayRoleOperation operation, string token);
    Task MarkDeliveredAsync(SnowflakeId guildId, BirthdayRoleOperation operation, string token);
}
