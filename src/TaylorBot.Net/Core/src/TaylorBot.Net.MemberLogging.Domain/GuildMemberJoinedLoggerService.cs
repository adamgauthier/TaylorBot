using Discord;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain;

public class GuildMemberJoinedLoggerService(
    MemberLogChannelFinder memberLogChannelFinder,
    TaskExceptionLogger taskExceptionLogger, BackgroundTasks backgroundTasks,
    GuildMemberJoinedEmbedFactory guildMemberJoinedEmbedFactory)
{
    public Task OnGuildMemberFirstJoinedAsync(IGuildUser guildUser)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            LogGuildMemberFirstJoinedAsync(guildUser),
            nameof(LogGuildMemberFirstJoinedAsync)
        ), nameof(GuildMemberJoinedLoggerService));
        return Task.CompletedTask;
    }

    private async Task LogGuildMemberFirstJoinedAsync(IGuildUser guildUser)
    {
        var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guildUser.Guild);

        if (logTextChannel != null)
            await logTextChannel.SendMessageAsync(embed: guildMemberJoinedEmbedFactory.CreateMemberFirstJoined(guildUser));
    }

    public Task OnGuildMemberRejoinedAsync(IGuildUser guildUser, DateTimeOffset firstJoinedAt)
    {
        _ = backgroundTasks.Queue(async () => await taskExceptionLogger.LogOnError(
            LogGuildMemberRejoinedAsync(guildUser, firstJoinedAt),
            nameof(LogGuildMemberRejoinedAsync)
        ), nameof(GuildMemberJoinedLoggerService));
        return Task.CompletedTask;
    }

    private async Task LogGuildMemberRejoinedAsync(IGuildUser guildUser, DateTimeOffset firstJoinedAt)
    {
        var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guildUser.Guild);

        if (logTextChannel != null)
            await logTextChannel.SendMessageAsync(embed: guildMemberJoinedEmbedFactory.CreateMemberRejoined(guildUser, firstJoinedAt));
    }
}
