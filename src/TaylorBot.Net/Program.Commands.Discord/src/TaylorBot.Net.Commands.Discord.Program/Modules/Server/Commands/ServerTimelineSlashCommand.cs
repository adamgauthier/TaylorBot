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

public class ServerTimelineSlashCommand(
    IServerJoinedRepository serverJoinedRepository,
    PageMessageFactory pageMessageFactory,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "server timeline";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);
                var guild = context.Guild;

                var timeline = await serverJoinedRepository.GetTimelineAsync(guild);

                var pages = timeline.Chunk(15).Select(entries => string.Join('\n', entries.Select(entry =>
                    $"{((DateTimeOffset)entry.first_joined_at).FormatLongDate()}: {entry.username.MdUserLink(entry.user_id)} is {((int)entry.rank).Ordinalize(TaylorBotCulture.Culture)} to join"
                ))).ToList();

                var baseEmbed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Member Joins Timeline");

                if (guild.Fetched != null)
                {
                    baseEmbed.WithGuildAsAuthor(guild.Fetched);
                }

                return pageMessageFactory.Create(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        pages,
                        hasPageFooter: true,
                        emptyText: "No joined dates found in this server 😕"
                    )),
                    IsCancellable: true
                ));
            },
            Preconditions: [inGuild.Create()]
        ));
    }
}
