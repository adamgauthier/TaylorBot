using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

public record UserBirthday(DateOnly Date, bool IsPrivate)
{
    public const int NoYearValue = 1804;

    public static readonly DateOnly ClearedDate = DateOnly.MinValue;

    public bool IsSet => Date != ClearedDate;
}

public record BirthdayCalendarEntry(SnowflakeId UserId, string Username, DateOnly NextBirthday);

public record AgeRole(SnowflakeId RoleId, int MinimumAge);

public interface IBirthdayRepository
{
    ValueTask<UserBirthday?> GetBirthdayAsync(DiscordUser user);
    ValueTask ClearBirthdayAsync(DiscordUser user);
    ValueTask SetBirthdayAsync(DiscordUser user, UserBirthday birthday);
    ValueTask<IList<BirthdayCalendarEntry>> GetBirthdayCalendarAsync(CommandGuild guild);
    ValueTask<IList<AgeRole>> GetAgeRolesAsync(SnowflakeId guildId);
}
