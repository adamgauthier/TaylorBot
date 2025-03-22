using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public class GenderClearSlashCommand(IGenderRepository genderRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "gender clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await genderRepository.ClearGenderAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your gender has been cleared. It will no longer be included in </server population:1137547317549998130> stats. ✅
                    You can set it again with {mention.SlashCommand("gender set", context)}.
                    """));
            }
        ));
    }
}
