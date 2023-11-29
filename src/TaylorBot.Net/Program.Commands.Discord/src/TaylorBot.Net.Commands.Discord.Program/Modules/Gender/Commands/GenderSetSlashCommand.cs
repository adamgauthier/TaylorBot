using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public class GenderSetSlashCommand : ISlashCommand<GenderSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("gender set");

    public record Options(ParsedString gender);

    private readonly IGenderRepository _genderRepository;

    public GenderSetSlashCommand(IGenderRepository genderRepository)
    {
        _genderRepository = genderRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _genderRepository.SetGenderAsync(context.User, options.gender.Value);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Your gender has been set to {options.gender.Value}. ✅
                    You are now included in </server population:1137547317549998130> stats for servers you're in. 🧮
                    People can now use {context.MentionCommand("gender show")} to see your gender. 👁️
                    """));
            }
        ));
    }
}
