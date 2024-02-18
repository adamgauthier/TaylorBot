using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Server.Domain;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public class ServerNamesSlashCommand(IGuildNamesRepository guildNamesRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server names");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;

                var guildNames = await guildNamesRepository.GetHistoryAsync(guild, 75);

                var guildNamesAsLines = guildNames.Select(n => $"{n.ChangedAt:MMMM dd, yyyy}: {n.GuildName}");

                var pages = guildNamesAsLines.Chunk(size: 15)
                    .Select(lines => string.Join('\n', lines))
                .ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithGuildAsAuthor(guild)
                    .WithTitle("Server Name History 🆔");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText: "No server name recorded. 😵")),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: [
                new InGuildPrecondition()
            ]
        ));
    }
}
