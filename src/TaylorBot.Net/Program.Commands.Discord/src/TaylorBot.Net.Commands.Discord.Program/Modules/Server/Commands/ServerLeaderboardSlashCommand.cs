using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.PageMessages;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public class ServerLeaderboardSlashCommand(IServerActivityRepository serverActivityRepository, MemberNotInGuildUpdater memberNotInGuildUpdater) : ISlashCommand<ServerLeaderboardSlashCommand.Options>
{
    public static string CommandName => "server leaderboard";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString @for);

    private record LeaderboardData(string Title, List<string> Pages);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

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
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
            ]
        ));
    }

    private async ValueTask<LeaderboardData> GetLeaderboardData(Options options, IGuild guild)
    {
        switch (options.@for.Value)
        {
            case "messages":
                {
                    var leaderboard = await serverActivityRepository.GetMessageLeaderboardAsync(guild);

                    memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                        nameof(ServerLeaderboardSlashCommand),
                        guild,
                        leaderboard.Select(e => new SnowflakeId(e.user_id)).ToList());

                    var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(entry =>
                        $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: **~{"message".ToQuantity(entry.message_count, $"{TaylorBotFormats.Readable}**")}"
                    ))).ToList();

                    return new LeaderboardData("Message Leaderboard 📚", pages);
                }

            case "minutes":
                {
                    var leaderboard = await serverActivityRepository.GetMinuteLeaderboardAsync(guild);

                    memberNotInGuildUpdater.UpdateMembersWhoLeftInBackground(
                        nameof(ServerLeaderboardSlashCommand),
                        guild,
                        leaderboard.Select(e => new SnowflakeId(e.user_id)).ToList());

                    var pages = leaderboard.Chunk(15).Select(entries => string.Join('\n', entries.Select(entry =>
                        $"{entry.rank}\\. {entry.username.MdUserLink(entry.user_id)}: {"minute".ToQuantity(entry.minute_count, TaylorBotFormats.BoldReadable)}"
                    ))).ToList();

                    return new LeaderboardData("Active Time Leaderboard ⏳", pages);
                }

            default: throw new NotImplementedException();
        }
    }
}
