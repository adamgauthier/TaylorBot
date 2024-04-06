using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

public interface IBirthdayRepository
{
    record Birthday(DateOnly Date, bool IsPrivate)
    {
        public const int NoYearValue = 1804;
    }

    record BirthdayCalendarEntry(SnowflakeId UserId, string Username, DateOnly NextBirthday);

    record AgeRole(SnowflakeId RoleId, int MinimumAge);

    ValueTask<Birthday?> GetBirthdayAsync(DiscordUser user);
    ValueTask ClearBirthdayAsync(DiscordUser user);
    ValueTask SetBirthdayAsync(DiscordUser user, Birthday birthday);
    ValueTask<IList<BirthdayCalendarEntry>> GetBirthdayCalendarAsync(CommandGuild guild);
    ValueTask<IList<AgeRole>> GetAgeRolesAsync(SnowflakeId guildId);
}
