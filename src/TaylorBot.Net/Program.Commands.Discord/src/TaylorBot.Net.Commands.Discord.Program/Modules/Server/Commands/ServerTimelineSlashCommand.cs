using Discord;
using Humanizer;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public class ServerTimelineSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server timeline");

    private readonly IServerJoinedRepository _serverJoinedRepository;

    public ServerTimelineSlashCommand(IServerJoinedRepository serverJoinedRepository)
    {
        _serverJoinedRepository = serverJoinedRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var timeline = await _serverJoinedRepository.GetTimelineAsync(guild);

                var pages = timeline.Chunk(15).Select(entries => string.Join('\n', entries.Select(entry =>
                    $"{((DateTimeOffset)entry.first_joined_at).FormatLongDate()}: {entry.username.MdUserLink(entry.user_id)} is {((int)entry.rank).Ordinalize(TaylorBotCulture.Culture)} to join"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithGuildAsAuthor(guild)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Member Joins Timeline");

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText: "No joined dates found in this server 😕"
                    )),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
            }
        ));
    }
}
