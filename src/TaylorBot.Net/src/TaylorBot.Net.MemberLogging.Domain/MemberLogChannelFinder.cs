using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Domain
{
    public class MemberLogChannelFinder
    {
        private readonly ILoggingTextChannelRepository loggingTextChannelRepository;

        public MemberLogChannelFinder(ILoggingTextChannelRepository loggingTextChannelRepository)
        {
            this.loggingTextChannelRepository = loggingTextChannelRepository;
        }

        public async Task<ITextChannel> FindLogChannelAsync(IGuild guild)
        {
            var logChannels = await loggingTextChannelRepository.GetLogChannelsForGuildAsync(guild);
            var textChannels = await guild.GetTextChannelsAsync();

            return textChannels.FirstOrDefault(channel =>
                logChannels.Any(logChannel => logChannel.ChannelId.Id == channel.Id)
            );
        }
    }
}
