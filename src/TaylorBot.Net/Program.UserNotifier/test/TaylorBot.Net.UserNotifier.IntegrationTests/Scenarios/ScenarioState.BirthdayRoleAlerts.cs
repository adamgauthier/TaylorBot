using TaylorBot.Net.UserNotifier.Program.Jobs;

namespace TaylorBot.Net.UserNotifier.IntegrationTests.Scenarios;

public sealed partial class ScenarioState
{
    internal static string BirthdayRoleAlertKey(ScenarioGuild guild, UserNotifierJob job) =>
        $"birthday-role-alert:guild:{guild.Id}:{(job == UserNotifierJob.BirthdayRoleAdd ? "Assign" : "Remove")}";

    public Task<TimeSpan?> BirthdayRoleAlertExpiryAsync(ScenarioGuild guild, UserNotifierJob job) =>
        database.Redis.KeyTimeToLiveAsync(BirthdayRoleAlertKey(guild, job));
}
