using Discord;
using Humanizer;
using Humanizer.Localisation;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public record ServerMessages(int message_count, int word_count);

public record MessageLeaderboardEntry(string user_id, string username, long rank, int message_count);

public record MinuteLeaderboardEntry(string user_id, string username, long rank, int minute_count);

public interface IServerActivityRepository
{
    Task<ServerMessages> GetMessagesAsync(DiscordMember member);
    Task<IList<MessageLeaderboardEntry>> GetMessageLeaderboardAsync(IGuild guild);
    Task<int> GetMinutesAsync(DiscordMember member);
    Task<IList<MinuteLeaderboardEntry>> GetMinuteLeaderboardAsync(IGuild guild);
    Task<int?> GetOldMinutesAsync(DiscordUser user);
}

public class ServerMessagesSlashCommand(
    IServerActivityRepository serverActivityRepository,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<ServerMessagesSlashCommand.Options>
{
    public static string CommandName => "server messages";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedMemberOrAuthor user);

    public Command Messages(DiscordMember member) => new(
        new("messages"),
        async () =>
        {
            var messages = await serverActivityRepository.GetMessagesAsync(member);
            var wordAverage = messages.message_count > 0 ? (double)messages.word_count / messages.message_count : 0;

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(member.User)
                .WithDescription(
                    $"""
                    {member.User.Mention} sent **~{"message".ToQuantity(messages.message_count, TaylorBotFormats.Readable)}** in this server. 📚
                    Each of their messages contains on average **{wordAverage:0.00}** words. ✍️

                    *Count is approximate and updated every few minutes.*
                    *Messages before TaylorBot was added to the server are not counted.*
                    *Messages in channels TaylorBot doesn't have access to are not counted.*
                    *Messages in channels marked as spam by moderators are not counted.*
                    """);

            return new EmbedResult(embed.Build());
        },
        [inGuild.Create()]
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Messages(options.user.Member));
    }
}

public class ServerMinutesSlashCommand(
    IServerActivityRepository serverActivityRepository,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<ServerMinutesSlashCommand.Options>
{
    public static string CommandName => "server minutes";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedMemberOrAuthor user);

    public Command Minutes(DiscordMember member, RunContext context) => new(
        new("minutes"),
        async () =>
        {
            var minutes = await serverActivityRepository.GetMinutesAsync(member);

            var bottomText = $"Check out {mention.SlashCommand("server leaderboard", context)} to see the most active server members! 📃";
            if (member.Member.GuildId == 115332333745340416)
            {
                var oldMinutes = await serverActivityRepository.GetOldMinutesAsync(member.User);
                if (oldMinutes != null)
                {
                    bottomText = $"Before **December 25th 2015**, minutes used to be counted based on online status. Under that system, this user had {"minute".ToQuantity(oldMinutes.Value, TaylorBotFormats.BoldReadable)}. 🧓";
                }
            }

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(member.User)
                .WithDescription(
                    $"""
                    {member.User.Mention} has been active for {"minute".ToQuantity(minutes, TaylorBotFormats.BoldReadable)} in this server. ⏳
                    This is roughly equivalent to **{TimeSpan.FromMinutes(minutes).Humanize(maxUnit: TimeUnit.Month, culture: TaylorBotCulture.Culture)}** of activity.

                    {bottomText}
                    """);

            return new EmbedResult(embed.Build());
        },
        [inGuild.Create()]
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Minutes(options.user.Member, context));
    }
}
