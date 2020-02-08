using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledGuildCommandRepository
    {
        Task<bool> IsGuildCommandDisabledAsync(IGuild guild, CommandInfo command);
    }

    public class RequireNotGuildDisabledAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null)
                return PreconditionResult.FromSuccess();

            var guildCommandDisabledRepository = services.GetRequiredService<IDisabledGuildCommandRepository>();

            var isDisabled = await guildCommandDisabledRepository.IsGuildCommandDisabledAsync(context.Guild, command);

            return isDisabled ?
                TaylorBotPreconditionResult.FromPrivateError($"{command.Name} is disabled in {context.Guild.FormatLog()}") :
                PreconditionResult.FromSuccess();
        }
    }
}
