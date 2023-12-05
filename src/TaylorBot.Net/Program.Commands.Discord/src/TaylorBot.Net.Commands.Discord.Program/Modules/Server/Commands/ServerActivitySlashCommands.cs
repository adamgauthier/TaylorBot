﻿using Discord;
using Humanizer;
using Humanizer.Localisation;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Server.Commands;

public record ServerMessages(int message_count, int word_count);

public record MessageLeaderboardEntry(string user_id, string username, long rank, int message_count);

public record MinuteLeaderboardEntry(string user_id, string username, long rank, int minute_count);

public interface IServerActivityRepository
{
    Task<ServerMessages> GetMessagesAsync(IGuildUser guildUser);
    Task<IList<MessageLeaderboardEntry>> GetMessageLeaderboardAsync(IGuild guild);
    Task<int> GetMinutesAsync(IGuildUser guildUser);
    Task<IList<MinuteLeaderboardEntry>> GetMinuteLeaderboardAsync(IGuild guild);
    Task<int?> GetOldMinutesAsync(IUser user);
}

public class ServerMessagesSlashCommand(IServerActivityRepository serverActivityRepository) : ISlashCommand<ServerMessagesSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server messages");

    public record Options(ParsedUserOrAuthor user);

    public Command Messages(IUser user, bool isLegacyCommand) => new(
        new("messages"),
        async () =>
        {
            var guildUser = (IGuildUser)user;
            var messages = await serverActivityRepository.GetMessagesAsync(guildUser);
            var wordAverage = (double)messages.word_count / messages.message_count;

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    {guildUser.Mention} sent **~{"message".ToQuantity(messages.message_count, TaylorBotFormats.Readable)}** in this server. 📚
                    Each of their messages contains on average **{wordAverage:0.00}** words. ✍️

                    *Count is approximate and updated every few minutes.*
                    *Messages before TaylorBot was added to the server are not counted.*
                    *Messages in channels TaylorBot doesn't have access to are not counted.*
                    *Messages in channels marked as spam by moderators are not counted.*
                    """);

            if (isLegacyCommand)
            {
                embed.WithUserAsAuthor(guildUser);
            }

            return new EmbedResult(embed.Build());
        },
        new ICommandPrecondition[] { new InGuildPrecondition() }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Messages(options.user.User, isLegacyCommand: false));
    }
}

public class ServerMinutesSlashCommand(IServerActivityRepository serverActivityRepository) : ISlashCommand<ServerMinutesSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("server minutes");

    public record Options(ParsedUserOrAuthor user);

    public Command Minutes(IUser user, bool isLegacyCommand) => new(
        new("minutes"),
        async () =>
        {
            var guildUser = (IGuildUser)user;
            var minutes = await serverActivityRepository.GetMinutesAsync(guildUser);

            var bottomText = "Check out </server leaderboard:1137547317549998130> to see the most active server members! 📃";
            if (guildUser.GuildId == 115332333745340416)
            {
                var oldMinutes = await serverActivityRepository.GetOldMinutesAsync(guildUser);
                if (oldMinutes != null)
                {
                    bottomText = $"Before **December 25th 2015**, minutes used to be counted based on online status. Under that system, this user had {"minute".ToQuantity(oldMinutes.Value, TaylorBotFormats.BoldReadable)}. 🧓";
                }
            }

            var embed = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(
                    $"""
                    {guildUser.Mention} has been active for {"minute".ToQuantity(minutes, TaylorBotFormats.BoldReadable)} in this server. ⏳
                    This is roughly equivalent to **{TimeSpan.FromMinutes(minutes).Humanize(maxUnit: TimeUnit.Month, culture: TaylorBotCulture.Culture)}** of activity.

                    {bottomText}
                    """);

            if (isLegacyCommand)
            {
                embed.WithUserAsAuthor(guildUser);
            }

            return new EmbedResult(embed.Build());
        },
        new ICommandPrecondition[] { new InGuildPrecondition() }
    );

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(Minutes(options.user.User, isLegacyCommand: false));
    }
}
