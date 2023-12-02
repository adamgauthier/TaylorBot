using Discord;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain;

public class GuildMemberJoinedLoggerService
{
    private readonly MemberLogChannelFinder memberLogChannelFinder;
    private readonly TaskExceptionLogger taskExceptionLogger;
    private readonly GuildMemberJoinedEmbedFactory guildMemberJoinedEmbedFactory;

    public GuildMemberJoinedLoggerService(
        MemberLogChannelFinder memberLogChannelFinder,
        TaskExceptionLogger taskExceptionLogger,
        GuildMemberJoinedEmbedFactory guildMemberJoinedEmbedFactory)
    {
        this.memberLogChannelFinder = memberLogChannelFinder;
        this.taskExceptionLogger = taskExceptionLogger;
        this.guildMemberJoinedEmbedFactory = guildMemberJoinedEmbedFactory;
    }

    public Task OnGuildMemberFirstJoinedAsync(IGuildUser guildUser)
    {
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            LogGuildMemberFirstJoinedAsync(guildUser),
            nameof(LogGuildMemberFirstJoinedAsync)
        ));
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
        Task.Run(async () => await taskExceptionLogger.LogOnError(
            LogGuildMemberRejoinedAsync(guildUser, firstJoinedAt),
            nameof(LogGuildMemberRejoinedAsync)
        ));
        return Task.CompletedTask;
    }

    private async Task LogGuildMemberRejoinedAsync(IGuildUser guildUser, DateTimeOffset firstJoinedAt)
    {
        var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guildUser.Guild);

        if (logTextChannel != null)
            await logTextChannel.SendMessageAsync(embed: guildMemberJoinedEmbedFactory.CreateMemberRejoined(guildUser, firstJoinedAt));
    }
}
