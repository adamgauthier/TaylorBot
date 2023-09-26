using Discord;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public class ServerLeaderboardSlashCommand : ISlashCommand<ServerLeaderboardSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server leaderboard");

    public record Options(ParsedString @for);

    private readonly IServerActivityRepository _serverMessageRepository;

    public ServerLeaderboardSlashCommand(IServerActivityRepository serverMessageRepository)
    {
        _serverMessageRepository = serverMessageRepository;
    }

    private record LeaderboardData(string Title, List<string> Pages);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;

                var leaderboardData = await GetLeaderboardData(options, guild);

                var baseEmbed = new EmbedBuilder()
                    .WithGuildAsAuthor(guild)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle(leaderboardData.Title);

                return new PageMessageResultBuilder(new(
                    new(new EmbedDescriptionTextEditor(
                        baseEmbed,
                        leaderboardData.Pages,
                        hasPageFooter: true,
                        emptyText: "No data found in this server 😕"
                    )),
                    IsCancellable: true
                )).Build();
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
            }
        ));
    }

    private async ValueTask<LeaderboardData> GetLeaderboardData(Options options, IGuild guild)
    {
        switch (options.@for.Value)
        {
            case "messages":
                {
                    var leaderboard = await _serverMessageRepository.GetMessageLeaderboardAsync(guild);

                    var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(entry =>
                        $"{entry.rank}. {entry.username.MdUserLink(entry.user_id)}: **~{"message".ToQuantity(entry.message_count, $"{TaylorBotFormats.Readable}**")}"
                    ))).ToList();

                    return new LeaderboardData("Message Leaderboard 📚", pages);
                }

            case "minutes":
                {
                    var leaderboard = await _serverMessageRepository.GetMinuteLeaderboardAsync(guild);

                    var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(entry =>
                        $"{entry.rank}. {entry.username.MdUserLink(entry.user_id)}: {"minute".ToQuantity(entry.minute_count, TaylorBotFormats.BoldReadable)}"
                    ))).ToList();

                    return new LeaderboardData("Active Time Leaderboard ⏳", pages);
                }

            default: throw new NotImplementedException();
        }
    }
}
