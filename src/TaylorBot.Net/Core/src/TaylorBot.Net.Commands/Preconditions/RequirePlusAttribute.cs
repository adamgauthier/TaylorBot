using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Preconditions
{
    public interface IPlusRepository
    {
        ValueTask<bool> IsActivePlusUserAsync(IUser user);
        ValueTask<bool> IsActivePlusGuildAsync(IGuild guild);
    }

    public enum PlusRequirement { PlusUser, PlusGuild, PlusUserOrGuild }

    public class RequirePlusAttribute : PreconditionAttribute
    {
        private readonly PlusRequirement _requirement;
        public RequirePlusAttribute(PlusRequirement requirement) => _requirement = requirement;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var plusRepository = services.GetRequiredService<IPlusRepository>();

            var plusInfo = string.Join('\n', new[] {
                $"TaylorBot is free and {"supported by the community on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}.",
                $"Some more demanding features are exclusive to **TaylorBot Plus**, learn more by using `{((TaylorBotShardedCommandContext)context).CommandPrefix}plus`."
            });

            return _requirement switch
            {
                PlusRequirement.PlusUser =>
                    await plusRepository.IsActivePlusUserAsync(context.User) ?
                    PreconditionResult.FromSuccess() :
                    TaylorBotPreconditionResult.FromUserError(
                        privateReason: $"{command.Aliases[0]} is restricted to plus users",
                        userReason: string.Join('\n', new[] {
                            $"You can't use `{command.Aliases[0]}` because it is restricted to **TaylorBot Plus** members.",
                            plusInfo
                        })
                    ),

                PlusRequirement.PlusGuild =>
                    await plusRepository.IsActivePlusGuildAsync(context.Guild) ?
                        PreconditionResult.FromSuccess() :
                        TaylorBotPreconditionResult.FromUserError(
                            privateReason: $"{command.Aliases[0]} is restricted to plus guilds",
                            userReason: string.Join('\n', new[] {
                                $"You can't use `{command.Aliases[0]}` because it is restricted to **TaylorBot Plus** servers.",
                                plusInfo
                            })
                        ),

                PlusRequirement.PlusUserOrGuild =>
                    await plusRepository.IsActivePlusGuildAsync(context.Guild) || await plusRepository.IsActivePlusUserAsync(context.User) ?
                        PreconditionResult.FromSuccess() :
                        TaylorBotPreconditionResult.FromUserError(
                            privateReason: $"{command.Aliases[0]} is restricted to plus users or guilds",
                            userReason: string.Join('\n', new[] {
                                $"You can't use `{command.Aliases[0]}` because it is restricted to **TaylorBot Plus** members or servers.",
                                plusInfo
                            })
                        ),

                _ => throw new NotImplementedException(),
            };
        }
    }
}
