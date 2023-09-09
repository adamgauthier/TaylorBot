using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Gender.Commands;

public class GenderShowSlashCommand : ISlashCommand<GenderShowSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("gender show");

    public record Options(ParsedUserOrAuthor user);

    private readonly IGenderRepository _genderRepository;

    public GenderShowSlashCommand(IGenderRepository genderRepository)
    {
        _genderRepository = genderRepository;
    }

    public Command Show(IUser user, RunContext? context = null) => new(
        new(Info.Name),
        async () =>
        {
            var gender = await _genderRepository.GetGenderAsync(user);

            if (gender != null)
            {
                var embed = new EmbedBuilder()
                    .WithUserAsAuthor(user)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        {user.Username}'s gender is **{gender}**. 🆔
                        """);

                return new EmbedResult(embed.Build());
            }
            else
            {
                return new EmbedResult(EmbedFactory.CreateError(
                    $"""
                    {user.Mention}'s gender is not set. 🚫
                    They need to use {context?.MentionCommand("gender set") ?? "</gender set:1150180971224764510>"} to set it first.
                    """));
            }
        }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Show(options.user.User, context));
    }
}
