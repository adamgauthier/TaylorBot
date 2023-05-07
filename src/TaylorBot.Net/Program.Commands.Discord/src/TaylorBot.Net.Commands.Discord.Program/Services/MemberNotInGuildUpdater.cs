using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain.Member;

namespace TaylorBot.Net.Commands.Discord.Program.Services;

public class MemberNotInGuildUpdater
{
    private readonly ITaylorBotClient _taylorBotClient;
    private readonly IMemberRepository _memberRepository;
    private readonly TaskExceptionLogger _taskExceptionLogger;

    public MemberNotInGuildUpdater(ITaylorBotClient taylorBotClient, IMemberRepository memberRepository, TaskExceptionLogger taskExceptionLogger)
    {
        _taylorBotClient = taylorBotClient;
        _memberRepository = memberRepository;
        _taskExceptionLogger = taskExceptionLogger;
    }

    public void UpdateMembersWhoLeftInBackground(string taskName, IGuild guild, IList<SnowflakeId> userIds)
    {
        _ = Task.Run(async () => await _taskExceptionLogger.LogOnError(
            async () => await UpdateMembersWhoLeft(guild, userIds),
            taskName
        ));
    }

    private async Task UpdateMembersWhoLeft(IGuild guild, IList<SnowflakeId> userIds)
    {
        List<SnowflakeId> membersNotInGuild = new();

        foreach (var userId in userIds)
        {
            var guildUser = await _taylorBotClient.ResolveGuildUserAsync(guild, userId);
            if (guildUser == null)
            {
                membersNotInGuild.Add(userId);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        await _memberRepository.UpdateMembersNotInGuildAsync(guild, membersNotInGuild);
    }
}
