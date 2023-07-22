using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Preconditions;

public interface IPlusRepository
{
    ValueTask<bool> IsActivePlusUserAsync(IUser user);
    ValueTask<bool> IsActivePlusGuildAsync(IGuild guild);
}

public enum PlusRequirement { PlusUser, PlusGuild, PlusUserOrGuild }

public class PlusPrecondition : ICommandPrecondition
{
    private readonly IPlusRepository _plusRepository;
    private readonly PlusRequirement _requirement;

    public PlusPrecondition(IPlusRepository plusRepository, PlusRequirement requirement)
    {
        _plusRepository = plusRepository;
        _requirement = requirement;
    }

    public async ValueTask<ICommandResult> CanRunAsync(Command command, RunContext context)
    {
        var plusInfo = string.Join('\n', new[] {
            $"TaylorBot is free and {"supported by the community on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}.",
            $"Some more demanding features are exclusive to **TaylorBot Plus**, learn more by using `{context.CommandPrefix}plus`."
        });

        return _requirement switch
        {
            PlusRequirement.PlusUser =>
                await _plusRepository.IsActivePlusUserAsync(context.User) ?
                new PreconditionPassed() :
                new PreconditionFailed(
                    PrivateReason: $"{command.Metadata.Name} is restricted to plus users",
                    UserReason: new(string.Join('\n', new[] {
                        $"You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** members.",
                        plusInfo
                    }))
                ),

            PlusRequirement.PlusGuild =>
                await _plusRepository.IsActivePlusGuildAsync(context.Guild!) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus guilds",
                        UserReason: new(string.Join('\n', new[] {
                            $"You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** servers.",
                            plusInfo
                        }))
                    ),

            PlusRequirement.PlusUserOrGuild =>
                await _plusRepository.IsActivePlusGuildAsync(context.Guild!) || await _plusRepository.IsActivePlusUserAsync(context.User) ?
                    new PreconditionPassed() :
                    new PreconditionFailed(
                        PrivateReason: $"{command.Metadata.Name} is restricted to plus users or guilds",
                        UserReason: new(string.Join('\n', new[] {
                            $"You can't use `{command.Metadata.Name}` because it is restricted to **TaylorBot Plus** members or servers.",
                            plusInfo
                        }))
                    ),

            _ => throw new NotImplementedException(),
        };
    }
}
