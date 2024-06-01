using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

public class PlusRemoveSlashCommand(IPlusRepository plusRepository, IPlusUserRepository plusUserRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("plus remove");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);

                await plusUserRepository.DisablePlusGuildAsync(context.User, context.Guild);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Successfully removed {context.Guild.Fetched?.Name ?? "this server"} from your plus servers ❌
                        Use {context.MentionCommand("plus add")} if you change your mind 😊
                        """)
                    .Build()
                );
            },
            Preconditions: [
                new InGuildPrecondition(),
                new PlusPrecondition(plusRepository, PlusRequirement.PlusUser),
            ]
        ));
    }
}
