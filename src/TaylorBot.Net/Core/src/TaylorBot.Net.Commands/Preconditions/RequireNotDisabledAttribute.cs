using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledCommandRepository
    {
        ValueTask<string> InsertOrGetCommandDisabledMessageAsync(CommandInfo command);
        ValueTask EnableGloballyAsync(string commandName);
        ValueTask<string> DisableGloballyAsync(string commandName, string disabledMessage);
    }

    public class RequireNotDisabledAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var commandDisabledRepository = services.GetRequiredService<IDisabledCommandRepository>();

            var disabledMessage = await commandDisabledRepository.InsertOrGetCommandDisabledMessageAsync(command);

            return disabledMessage != string.Empty ?
                TaylorBotPreconditionResult.FromUserError(
                    privateReason: $"{command.Aliases.First()} is globally disabled",
                    userReason: string.Join('\n', new[] {
                        $"You can't use `{command.Aliases.First()}` because it is globally disabled right now.",
                        disabledMessage
                    })
                ) :
                PreconditionResult.FromSuccess();
        }
    }
}
