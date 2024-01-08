using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IMemberTrackingRepository
{
    ValueTask<bool> AddOrUpdateMemberAsync(IGuildUser member, DateTimeOffset? lastSpokeAt);
}

public class MemberTrackedPrecondition(ILogger<MemberTrackedPrecondition> logger, IMemberTrackingRepository memberRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.User is not IGuildUser guildUser)
            return new PreconditionPassed();

        var memberAdded = await memberRepository.AddOrUpdateMemberAsync(guildUser, lastSpokeAt: context.CreatedAt);

        if (memberAdded)
        {
            logger.LogInformation("Added new member {GuildUser}.", guildUser.FormatLog());
        }

        return new PreconditionPassed();
    }
}
