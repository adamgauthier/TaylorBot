using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IPlusRepository
{
    ValueTask<bool> IsActivePlusUserAsync(DiscordUser user);
    ValueTask<bool> IsActivePlusGuildAsync(CommandGuild guild);
}

public enum PlusRequirement { PlusUser, PlusGuild, PlusUserOrGuild }

public class PlusPrecondition(IPlusRepository plusRepository, PlusRequirement requirement) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        return requirement switch
        {
            PlusRequirement.PlusUser =>
                await plusRepository.IsActivePlusUserAsync(context.User) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus users",
                        UserReason: new(
                            $"""
                            You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** members.
                            {await PlusInfoAsync(context)}
                            """)
                    ),

            PlusRequirement.PlusGuild =>
                await IsPlusGuildAsync(context) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus guilds",
                        UserReason: new(
                            $"""
                            You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** servers.
                            {await PlusInfoAsync(context)}
                            """)
                    ),

            PlusRequirement.PlusUserOrGuild =>
                await IsPlusGuildAsync(context) || await plusRepository.IsActivePlusUserAsync(context.User) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus users or guilds",
                        UserReason: new(
                            $"""
                            You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** members or servers.
                            {await PlusInfoAsync(context)}
                            """)
                    ),

            _ => throw new NotImplementedException(),
        };
    }

    private async Task<bool> IsPlusGuildAsync(RunContext context)
    {
        return context.Guild != null && await plusRepository.IsActivePlusGuildAsync(context.Guild);
    }

    private static async Task<string> PlusInfoAsync(RunContext context)
    {
        return
            $"""
            TaylorBot is free and {"supported by the community on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}.
            Some features are exclusive to **TaylorBot Plus**, learn more by using `{await context.CommandPrefix.Value}plus`.
            """;
    }
}
