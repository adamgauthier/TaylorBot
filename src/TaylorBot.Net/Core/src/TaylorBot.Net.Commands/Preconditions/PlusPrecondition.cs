using Discord;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IPlusRepository
{
    ValueTask<bool> IsActivePlusUserAsync(IUser user);
    ValueTask<bool> IsActivePlusGuildAsync(IGuild guild);
}

public enum PlusRequirement { PlusUser, PlusGuild, PlusUserOrGuild }

public class PlusPrecondition(IPlusRepository plusRepository, PlusRequirement requirement) : ICommandPrecondition
{
    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var plusInfo =
            $"""
            TaylorBot is free and {"supported by the community on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}.
            Some features are exclusive to **TaylorBot Plus**, learn more by using `{context.CommandPrefix}plus`.
            """;

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
                        {plusInfo}
                        """)
                ),

            PlusRequirement.PlusGuild =>
                await plusRepository.IsActivePlusGuildAsync(context.Guild!) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus guilds",
                        UserReason: new(
                            $"""
                            You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** servers.
                            {plusInfo}
                            """)
                    ),

            PlusRequirement.PlusUserOrGuild =>
                await plusRepository.IsActivePlusGuildAsync(context.Guild!) || await plusRepository.IsActivePlusUserAsync(context.User) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus users or guilds",
                        UserReason: new(
                            $"""
                            You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** members or servers.
                            {plusInfo}
                            """)
                    ),

            _ => throw new NotImplementedException(),
        };
    }
}
