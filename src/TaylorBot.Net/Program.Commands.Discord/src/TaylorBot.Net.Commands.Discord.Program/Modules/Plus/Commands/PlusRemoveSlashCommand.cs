using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

public class PlusRemoveSlashCommand(
    IPlusUserRepository plusUserRepository,
    PlusPrecondition.Factory plusPrecondition,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "plus remove";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

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
                        Use {mention.SlashCommand("plus add", context)} if you change your mind 😊
                        """)
                    .Build()
                );
            },
            Preconditions: [
                inGuild.Create(),
                plusPrecondition.Create(PlusRequirement.PlusUser),
            ]
        ));
    }
}
