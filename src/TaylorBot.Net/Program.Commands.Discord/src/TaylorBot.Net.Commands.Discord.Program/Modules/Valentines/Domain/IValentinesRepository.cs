using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Valentines.Domain
{
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
        ValueTask<RoleObtained?> GetRoleObtainedByUserAsync(IGuildUser user);
        ValueTask<DateTimeOffset> SpreadRoleAsync(IGuildUser fromUser, IGuildUser toUser);
        ValueTask<IReadOnlyList<RoleObtained>> GetRoleObtainedFromUserAsync(IGuildUser user);
        ValueTask<IReadOnlyList<RoleObtained>> GetAllAsync();
        ValueTask<IReadOnlyList<RoleObtained>> GetAllReadyAsync(ValentinesConfig config);
    }
}
