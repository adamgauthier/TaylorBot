using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Preconditions
{
    public class RequireTextChannelTrackedAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is ITextChannel textChannel)
            {
                await services.GetRequiredService<ISpamChannelRepository>().InsertOrGetIsSpamChannelAsync(textChannel);
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
