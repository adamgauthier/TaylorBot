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
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public record ServerJoined(DateTime? first_joined_at);

public record JoinedTimelineEntry(string user_id, string username, DateTime first_joined_at, long rank);

public interface IServerJoinedRepository
{
    Task<ServerJoined> GetRankedJoinedAsync(DiscordMember guildUser);
    Task FixMissingJoinedDateAsync(DiscordMember guildUser);
    Task<IList<JoinedTimelineEntry>> GetTimelineAsync(CommandGuild guild);
}

public class ServerJoinedSlashCommand(IServerJoinedRepository serverJoinedRepository) : ISlashCommand<ServerJoinedSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server joined");

    public record Options(ParsedMemberOrAuthor user);

    private async Task<ServerJoined> GetServerJoinedAsync(DiscordMember guildUser)
    {
        var joined = await serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        if (joined.first_joined_at == null && guildUser.Member.JoinedAt is not null)
        {
            await serverJoinedRepository.FixMissingJoinedDateAsync(guildUser);
            joined = await serverJoinedRepository.GetRankedJoinedAsync(guildUser);
        }
        return joined;
    }

    public Command Joined(DiscordMember member) => new(
        new("joined"),
        async () =>
        {
            var joined = await GetServerJoinedAsync(member);
            DateTimeOffset joinedAt = joined.first_joined_at ?? throw new InvalidOperationException();

            var sinceCreation = joinedAt - SnowflakeUtils.FromSnowflake(member.Member.GuildId);

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(member.User)
                .WithDescription(
                    $"""
                    {member.User.Mention} first joined on {joinedAt.FormatDetailedWithRelative()} 🚪
                    This was roughly **{sinceCreation.Humanize(maxUnit: TimeUnit.Year, culture: TaylorBotCulture.Culture)}** after the server was created 📆

                    Check out </server timeline:1137547317549998130> for a history of who joined first! 📃
                    """);

            return new EmbedResult(embed.Build());
        },
        [new InGuildPrecondition()]
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Joined(options.user.Member));
    }
}
