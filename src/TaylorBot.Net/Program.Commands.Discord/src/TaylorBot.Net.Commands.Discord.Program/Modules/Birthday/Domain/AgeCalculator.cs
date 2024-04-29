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

    private async Task TryAddAgeRolesAsync(DiscordMember user, int age)
    {
        var ageRoles = await birthdayRepository.GetAgeRolesAsync(user.Member.GuildId);

        foreach (var ageRole in ageRoles.Where(a =>
            age >= a.MinimumAge &&
            !user.Member.Roles.Contains(a.RoleId)))
        {
            await client.Value.AddRoleAsync(user.Member.GuildId, user.User.Id, ageRole.RoleId, new()
            {
                AuditLogReason = $"Assigned age role on user's birthday command ({age} years old)"
            });
        }
    }

    public void TryAddAgeRolesInBackground(RunContext context, DiscordUser user, int age)
    {
        if (context.Guild?.Fetched != null && user.TryGetMember(out var member))
        {
            _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
                TryAddAgeRolesAsync(member, age),
                nameof(TryAddAgeRolesAsync))
            );
        }
    }
}
