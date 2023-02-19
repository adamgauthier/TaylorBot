using System;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain
{
    public class AgeCalculator
    {
        public static int GetCurrentAge(DateTimeOffset now, DateOnly birthday)
        {
            var today = now.Date;
            var age = today.Year - birthday.Year;

            if (birthday.ToDateTime(TimeOnly.MinValue) > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
