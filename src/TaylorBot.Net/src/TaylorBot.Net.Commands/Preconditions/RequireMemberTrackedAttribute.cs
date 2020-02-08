using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IMemberRepository
    {
        Task<bool> AddOrUpdateMemberAsync(IGuildUser member);
    }

    public class RequireMemberTrackedAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!(context.User is IGuildUser guildUser))
                return PreconditionResult.FromSuccess();

            var memberRepository = services.GetRequiredService<IMemberRepository>();

            var memberAdded = await memberRepository.AddOrUpdateMemberAsync(guildUser);

            if (memberAdded)
            {
                services.GetRequiredService<ILogger<RequireMemberTrackedAttribute>>().LogInformation(LogString.From(
                    $"Added new member {guildUser.FormatLog()}."
                ));
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
