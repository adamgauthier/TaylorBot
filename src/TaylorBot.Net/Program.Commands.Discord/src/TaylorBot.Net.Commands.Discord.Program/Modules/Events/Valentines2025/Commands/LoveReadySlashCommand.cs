using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Events.Valentines2025.Commands;

public class LoveReadySlashCommand(IValentinesRepository valentinesRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "love ready";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var config = await valentinesRepository.GetConfigurationAsync();
                var ready = await valentinesRepository.GetAllReadyAsync(config);

                var obtainedAsLines = ready.Select(o => $"{o.ToUserName}");

                var pages =
                    obtainedAsLines.Chunk(size: 15)
                    .Select(lines => string.Join('\n', lines))
                    .ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Members that are ready to spread love");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText: "There are no members ready to spread love! 😭"
                    ))
                )).Build();
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
            ]
        ));
    }
}
