using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledCommandRepository
    {
        Task<bool> InsertOrGetIsCommandDisabledAsync(CommandInfo command);
    }

    public class RequireNotDisabledAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var commandDisabledRepository = services.GetRequiredService<IDisabledCommandRepository>();

            var isDisabled = await commandDisabledRepository.InsertOrGetIsCommandDisabledAsync(command);

            return isDisabled ?
                TaylorBotPreconditionResult.FromUserError(
                    privateReason: $"{command.Name} is globally disabled",
                    userReason: $"You can't use `{command.Name}` because it is globally disabled right now. Please check back later."
                ) :
                PreconditionResult.FromSuccess();
        }
    }
}
