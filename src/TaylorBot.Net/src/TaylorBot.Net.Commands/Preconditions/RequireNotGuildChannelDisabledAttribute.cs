using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IDisabledGuildChannelCommandRepository
    {
        Task<bool> IsGuildChannelCommandDisabledAsync(ITextChannel textChannel, CommandInfo command);
    }

    public class RequireNotGuildChannelDisabledAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!(context.Channel is ITextChannel textChannel))
                return PreconditionResult.FromSuccess();

            var commandContext = (TaylorBotShardedCommandContext)context;

            var guildChannelCommandDisabledRepository = services.GetRequiredService<IDisabledGuildChannelCommandRepository>();

            var isDisabled = await guildChannelCommandDisabledRepository.IsGuildChannelCommandDisabledAsync(textChannel, command);

            return isDisabled ?
                TaylorBotPreconditionResult.FromUserError(
                    privateReason: $"{command.Name} is disabled in {textChannel.FormatLog()}",
                    userReason: string.Join('\n', new[] {
                        $"You can't use `{command.Name}` because it is disabled in {textChannel.Mention}.",
                        $"You can re-enable it by typing `{commandContext.CommandPrefix}ecc {command.Name}`."
                    })
                ) :
                PreconditionResult.FromSuccess();
        }
    }
}
