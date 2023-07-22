using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

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

    public static async Task TryAddAgeRolesAsync(IBirthdayRepository birthdayRepository, IUser user, int age)
    {
        if (user is IGuildUser guildUser)
        {
            var ageRoles = await birthdayRepository.GetAgeRolesAsync(guildUser.Guild);

            foreach (var ageRole in ageRoles.Where(a =>
                age >= a.MinimumAge &&
                !guildUser.RoleIds.Contains(a.RoleId)))
            {
                var role = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == ageRole.RoleId);
                if (role != null)
                {
                    await guildUser.AddRoleAsync(role, new() { AuditLogReason = $"Assigned age role on user's birthday command ({age} years old)" });
                }
            }
        }
    }
}
