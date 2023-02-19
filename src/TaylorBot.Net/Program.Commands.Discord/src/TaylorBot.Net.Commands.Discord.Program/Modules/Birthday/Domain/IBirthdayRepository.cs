using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain
{
    public interface IBirthdayRepository
    {
        record Birthday(DateOnly Date, bool IsPrivate)
        {
            public const int NoYearValue = 1804;
        }
        record BirthdayCalendarEntry(SnowflakeId UserId, string Username, DateOnly NextBirthday);

        ValueTask<Birthday?> GetBirthdayAsync(IUser user);
        ValueTask ClearBirthdayAsync(IUser user);
        ValueTask SetBirthdayAsync(IUser user, Birthday birthday);
        ValueTask ClearLegacyAgeAsync(IUser user);
        ValueTask<IList<BirthdayCalendarEntry>> GetBirthdayCalendarAsync(IGuild guild);
    }
}
