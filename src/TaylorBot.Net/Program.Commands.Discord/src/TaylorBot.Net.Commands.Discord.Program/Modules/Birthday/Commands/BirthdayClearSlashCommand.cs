using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;

public class BirthdayClearSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("birthday clear");

    private readonly IBirthdayRepository _birthdayRepository;

    public BirthdayClearSlashCommand(IBirthdayRepository birthdayRepository)
    {
        _birthdayRepository = birthdayRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _birthdayRepository.ClearBirthdayAsync(context.User);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Your birthday has been cleared. Calendar, horoscope, age and birthday taypoints will no longer work. ✅
                        You can set it again with {context.MentionCommand("birthday set")}.
                        """);

                return new EmbedResult(embed.Build());
            }
        ));
    }
}
