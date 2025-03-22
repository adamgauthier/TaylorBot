using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayClearSlashCommand(IBirthdayRepository birthdayRepository, CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "birthday clear";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await birthdayRepository.ClearBirthdayAsync(context.User);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your birthday has been cleared. Calendar, horoscope, age and birthday taypoints will no longer work. ✅
                    You can set it again with {mention.SlashCommand("birthday set", context)}.
                    """));
            }
        ));
    }
}
