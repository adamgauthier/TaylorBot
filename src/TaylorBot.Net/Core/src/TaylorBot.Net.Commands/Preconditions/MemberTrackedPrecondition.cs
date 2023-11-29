using Discord;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IMemberTrackingRepository
{
    ValueTask<bool> AddOrUpdateMemberAsync(IGuildUser member, DateTimeOffset? lastSpokeAt);
}

public class MemberTrackedPrecondition : ICommandPrecondition
{
    private readonly ILogger<MemberTrackedPrecondition> _logger;
    private readonly IMemberTrackingRepository _memberRepository;

    public MemberTrackedPrecondition(ILogger<MemberTrackedPrecondition> logger, IMemberTrackingRepository memberRepository)
    {
        _logger = logger;
        _memberRepository = memberRepository;
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        if (context.User is not IGuildUser guildUser)
            return new PreconditionPassed();

        var memberAdded = await _memberRepository.AddOrUpdateMemberAsync(guildUser, lastSpokeAt: context.CreatedAt);

        if (memberAdded)
        {
            _logger.LogInformation("Added new member {GuildUser}.", guildUser.FormatLog());
        }

        return new PreconditionPassed();
    }
}
