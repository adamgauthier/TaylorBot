using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.ChannelLogging.Domain.DiscordEmbed;
using TaylorBot.Net.Core.Tasks;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.ChannelLogging.Domain
{
    public class GuildMemberJoinedLoggerService
    {
        private readonly ILoggingTextChannelRepository loggingTextChannelRepository;
        private readonly TaskExceptionLogger taskExceptionLogger;
        private readonly GuildMemberJoinedEmbedFactory guildMemberJoinedEmbedFactory;

        public GuildMemberJoinedLoggerService(
            ILoggingTextChannelRepository loggingTextChannelRepository,
            TaskExceptionLogger taskExceptionLogger,
            GuildMemberJoinedEmbedFactory guildMemberJoinedEmbedFactory)
        {
            this.loggingTextChannelRepository = loggingTextChannelRepository;
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
            var logTextChannel = await FindLogChannelAsync(guildUser.Guild);

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
            var logTextChannel = await FindLogChannelAsync(guildUser.Guild);

            if (logTextChannel != null)
                await logTextChannel.SendMessageAsync(embed: guildMemberJoinedEmbedFactory.CreateMemberRejoined(guildUser, firstJoinedAt));
        }

        private async Task<ITextChannel> FindLogChannelAsync(IGuild guild)
        {
            var logChannels = await loggingTextChannelRepository.GetLogChannelsForGuildAsync(guild);
            var textChannels = await guild.GetTextChannelsAsync();

            return textChannels.FirstOrDefault(channel =>
                logChannels.Any(logChannel => logChannel.ChannelId.Id == channel.Id)
            );
        }
    }
}
