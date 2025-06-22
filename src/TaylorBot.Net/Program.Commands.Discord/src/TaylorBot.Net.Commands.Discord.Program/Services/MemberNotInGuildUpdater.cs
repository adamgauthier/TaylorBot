using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain.Member;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class MemberNotInGuildUpdater(ILogger<MemberNotInGuildUpdater> logger, ITaylorBotClient taylorBotClient, IMemberRepository memberRepository, TaskExceptionLogger taskExceptionLogger)
{
    public void UpdateMembersWhoLeftInBackground(string taskName, IGuild guild, IReadOnlyList<SnowflakeId> userIds)
    {
        _ = Task.Run(async () => await taskExceptionLogger.LogOnError(
            async () => await UpdateMembersWhoLeft(taskName, guild, userIds),
            taskName
        ));
    }

    private async Task UpdateMembersWhoLeft(string taskName, IGuild guild, IReadOnlyList<SnowflakeId> userIds)
    {
        List<SnowflakeId> membersNotInGuild = [];

        foreach (var userId in userIds)
        {
            var guildUser = await guild.GetUserAsync(userId.Id).ConfigureAwait(false);

            if (guildUser == null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                guildUser = await taylorBotClient.RestClient.GetGuildUserAsync(guild.Id, userId.Id);
            }

            if (guildUser == null)
            {
                membersNotInGuild.Add(userId);
            }
        }

        if (membersNotInGuild.Count > 0)
        {
            logger.LogDebug("{TaskName}: Updating {Count}/{Total} members not in guild anymore", taskName, membersNotInGuild.Count, userIds.Count);
            await memberRepository.UpdateMembersNotInGuildAsync(guild, membersNotInGuild);
        }
        else
        {
            logger.LogDebug("{TaskName}: All {Total} members are still in guild", taskName, userIds.Count);
        }
    }
}
