﻿using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IMemberTrackingRepository
{
    ValueTask<bool> AddOrUpdateMemberAsync(DiscordMember member, DateTimeOffset? lastSpokeAt);
}

public class MemberTrackedPrecondition(
    ILogger<MemberTrackedPrecondition> logger,
    IMemberTrackingRepository memberRepository,
    CommandPrefixDomainService commandPrefixDomainService) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        if (guild == null)
        {
            return new PreconditionPassed();
        }

        // Bot is joined to the current guild, make sure it is tracked by resolving prefix from database
        _ = await commandPrefixDomainService.GetPrefixAsync(guild);

        if (!context.User.TryGetMember(out var member))
        {
            return new PreconditionPassed();
        }

        var memberAdded = await memberRepository.AddOrUpdateMemberAsync(member, lastSpokeAt: context.CreatedAt);

        if (memberAdded)
        {
            logger.LogInformation("Added new member {GuildUser}.", context.User.FormatLog());
        }

        return new PreconditionPassed();
    }
}
