using Discord;
using Humanizer;
using Humanizer.Localisation;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public record ServerJoined(DateTime? first_joined_at);

public record JoinedTimelineEntry(string user_id, string username, DateTime first_joined_at, long rank);

public interface IServerJoinedRepository
{
    Task<ServerJoined> GetRankedJoinedAsync(IGuildUser guildUser);
    Task FixMissingJoinedDateAsync(IGuildUser guildUser);
    Task<IList<JoinedTimelineEntry>> GetTimelineAsync(IGuild guild);
}

public class ServerJoinedSlashCommand(IServerJoinedRepository serverJoinedRepository) : ISlashCommand<ServerJoinedSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server joined");

    public record Options(ParsedUserOrAuthor user);

    private async Task<ServerJoined> GetServerJoinedAsync(IGuildUser guildUser)
    {
        var joined = await serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        if (joined.first_joined_at == null && guildUser.JoinedAt is not null)
        {
            await serverJoinedRepository.FixMissingJoinedDateAsync(guildUser);
            joined = await serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        }
        return joined;
    }

    public Command Joined(IUser user) => new(
        new("joined"),
        async () =>
        {
            var guildUser = (IGuildUser)user;
            ServerJoined joined = await GetServerJoinedAsync(guildUser);
            DateTimeOffset joinedAt = joined.first_joined_at ?? throw new InvalidOperationException();

            var sinceCreation = joinedAt - guildUser.Guild.CreatedAt;

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(guildUser)
                .WithDescription(
                    $"""
                    {guildUser.Mention} first joined on {joinedAt.FormatDetailedWithRelative()} 🚪
                    This was roughly **{sinceCreation.Humanize(maxUnit: TimeUnit.Year, culture: TaylorBotCulture.Culture)}** after the server was created 📆

                    Check out </server timeline:1137547317549998130> for a history of who joined first! 📃
                    """);

            return new EmbedResult(embed.Build());
        },
        [new InGuildPrecondition()]
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Joined(options.user.User));
    }
}
