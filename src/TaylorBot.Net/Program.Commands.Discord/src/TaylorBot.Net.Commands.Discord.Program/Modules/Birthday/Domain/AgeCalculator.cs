using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;

public class AgeCalculator(TaskExceptionLogger taskExceptionLogger, Lazy<ITaylorBotClient> client, IBirthdayRepository birthdayRepository)
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

    private async Task TryAddAgeRolesAsync(DiscordUser user, int age)
    {
        if (user.MemberInfo != null)
        {
            var ageRoles = await birthdayRepository.GetAgeRolesAsync(user.MemberInfo.GuildId);

            foreach (var ageRole in ageRoles.Where(a =>
                age >= a.MinimumAge &&
                !user.MemberInfo.Roles.Contains(a.RoleId)))
            {
                await client.Value.AddRoleAsync(user.MemberInfo.GuildId, user.Id, ageRole.RoleId, new()
                {
                    AuditLogReason = $"Assigned age role on user's birthday command ({age} years old)"
                });
            }
        }
    }

    public void TryAddAgeRolesInBackground(DiscordUser user, int age)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            TryAddAgeRolesAsync(user, age),
            nameof(TryAddAgeRolesAsync))
        );
    }
}
