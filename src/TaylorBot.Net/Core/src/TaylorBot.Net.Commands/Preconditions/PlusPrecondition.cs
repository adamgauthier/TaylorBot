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
                            You can't use {context.MentionCommand(command)} because it is restricted to **TaylorBot Plus** members 😕
                            {PlusInfo()}
                            """)
                    ),

            PlusRequirement.PlusGuild =>
                await IsPlusGuildAsync(context) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus guilds",
                        UserReason: new(
                            $"""
                            You can't use {context.MentionCommand(command)} because it is restricted to **TaylorBot Plus** servers 😕
                            {PlusInfo()}
                            """)
                    ),

            PlusRequirement.PlusUserOrGuild =>
                await IsPlusGuildAsync(context) || await plusRepository.IsActivePlusUserAsync(context.User) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus users or guilds",
                        UserReason: new(
                            $"""
                            You can't use {context.MentionCommand(command)} because it is restricted to **TaylorBot Plus** members or servers 😕
                            {PlusInfo()}
                            """)
                    ),

            _ => throw new NotImplementedException(),
        };
    }

    private async Task<bool> IsPlusGuildAsync(RunContext context)
    {
        return context.Guild != null && await plusRepository.IsActivePlusGuildAsync(context.Guild);
    }

    private static string PlusInfo()
    {
        return
            $"""
            TaylorBot is free and {"supported by the community on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")} 💖
            Some features are exclusive to **TaylorBot Plus**, learn more with </plus show:1246970937321066608> 💎
            """;
    }
}
