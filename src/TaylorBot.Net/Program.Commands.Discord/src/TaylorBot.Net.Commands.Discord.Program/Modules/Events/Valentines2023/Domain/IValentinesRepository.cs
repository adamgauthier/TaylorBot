using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2023.Domain;

public record ValentinesConfig(
    SnowflakeId SpreadLoveRoleId,
    TimeSpan IncubationPeriod,
    IReadOnlyList<SnowflakeId> BypassSpreadLimitRoleIds,
    int SpreadLimit,
    SnowflakeId LoungeChannelId,
    DateTimeOffset GiveawaysEndTime,
    TimeSpan TimeSpanBetweenGiveaways,
    int GiveawayTaypointPrizeMin,
    int GiveawayTaypointPrizeMax
);

public record RoleObtained(SnowflakeId FromUserId, string FromName, SnowflakeId ToUserId, string ToUserName, DateTimeOffset AcquiredAt);

public interface IValentinesRepository
{
    ValueTask<ValentinesConfig> GetConfigurationAsync();
    ValueTask<RoleObtained?> GetRoleObtainedByUserAsync(DiscordMember user);
    ValueTask<DateTimeOffset> SpreadRoleAsync(DiscordMember fromUser, DiscordMember toUser);
    ValueTask<IReadOnlyList<RoleObtained>> GetRoleObtainedFromUserAsync(DiscordUser user);
    ValueTask<IReadOnlyList<RoleObtained>> GetAllAsync();
    ValueTask<IReadOnlyList<RoleObtained>> GetAllReadyAsync(ValentinesConfig config);
}
