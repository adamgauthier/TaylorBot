using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IMemberTrackingRepository
{
    ValueTask<bool> AddOrUpdateMemberAsync(DiscordMember member, DateTimeOffset? lastSpokeAt);
}

public class MemberTrackedPrecondition(ILogger<MemberTrackedPrecondition> logger, IMemberTrackingRepository memberRepository) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.User.MemberInfo == null)
        {
            return new PreconditionPassed();
        }

        var memberAdded = await memberRepository.AddOrUpdateMemberAsync(new(context.User, context.User.MemberInfo), lastSpokeAt: context.CreatedAt);

        if (memberAdded)
        {
            logger.LogInformation("Added new member {GuildUser}.", context.User.FormatLog());
        }

        return new PreconditionPassed();
    }
}
