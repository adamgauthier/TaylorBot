using Discord;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain.Member;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class MemberNotInGuildUpdater(ITaylorBotClient taylorBotClient, IMemberRepository memberRepository, TaskExceptionLogger taskExceptionLogger)
{
    public void UpdateMembersWhoLeftInBackground(string taskName, IGuild guild, IReadOnlyList<SnowflakeId> userIds)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            async () => await UpdateMembersWhoLeft(guild, userIds),
            taskName
        ));
    }

    private async Task UpdateMembersWhoLeft(IGuild guild, IReadOnlyList<SnowflakeId> userIds)
    {
        List<SnowflakeId> membersNotInGuild = [];

        foreach (var userId in userIds)
        {
            var guildUser = await taylorBotClient.ResolveGuildUserAsync(guild, userId);
            if (guildUser == null)
            {
                membersNotInGuild.Add(userId);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        await memberRepository.UpdateMembersNotInGuildAsync(guild, membersNotInGuild);
    }
}
