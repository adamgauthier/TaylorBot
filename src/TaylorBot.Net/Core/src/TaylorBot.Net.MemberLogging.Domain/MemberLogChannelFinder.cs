using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Domain
{
    public class MemberLogChannelFinder
    {
        private readonly IMemberLoggingChannelRepository loggingTextChannelRepository;

        public MemberLogChannelFinder(IMemberLoggingChannelRepository loggingTextChannelRepository)
        {
            this.loggingTextChannelRepository = loggingTextChannelRepository;
        }

        public async ValueTask<ITextChannel?> FindLogChannelAsync(IGuild guild)
        {
            var logChannels = await loggingTextChannelRepository.GetLogChannelsForGuildAsync(guild);
            var textChannels = await guild.GetTextChannelsAsync();

            return textChannels.FirstOrDefault(channel =>
                logChannels.Any(logChannel => logChannel.ChannelId.Id == channel.Id)
            );
        }
    }
}
