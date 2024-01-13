using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

public interface IBirthdayRepository
{
    record Birthday(DateOnly Date, bool IsPrivate)
    {
        public const int NoYearValue = 1804;
    }

    record BirthdayCalendarEntry(SnowflakeId UserId, string Username, DateOnly NextBirthday);

    record AgeRole(SnowflakeId RoleId, int MinimumAge);

    ValueTask<Birthday?> GetBirthdayAsync(IUser user);
    ValueTask ClearBirthdayAsync(IUser user);
    ValueTask SetBirthdayAsync(IUser user, Birthday birthday);
    ValueTask<IList<BirthdayCalendarEntry>> GetBirthdayCalendarAsync(IGuild guild);
    ValueTask<IList<AgeRole>> GetAgeRolesAsync(IGuild guild);
}
