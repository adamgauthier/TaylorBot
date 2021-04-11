using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IMemberRepository
    {
        ValueTask<bool> AddOrUpdateMemberAsync(IGuildUser member, DateTimeOffset? lastSpokeAt);
    }

    public class MemberTrackedPrecondition : ICommandPrecondition
    {
        private readonly ILogger<MemberTrackedPrecondition> _logger;
        private readonly IMemberRepository _memberRepository;

        public MemberTrackedPrecondition(ILogger<MemberTrackedPrecondition> logger, IMemberRepository memberRepository)
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
                _logger.LogInformation($"Added new member {guildUser.FormatLog()}.");
            }

            return new PreconditionPassed();
        }
    }
}
