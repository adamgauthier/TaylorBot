using Discord;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.MemberLogging.Domain.TextChannel;

namespace TaylorBot.Net.MemberLogging.Domain
{
    public class MemberLogChannelFinder
    {
        private readonly IMemberLoggingChannelRepository _memberLoggingChannelRepository;

        public MemberLogChannelFinder(IMemberLoggingChannelRepository memberLoggingChannelRepository)
        {
            _memberLoggingChannelRepository = memberLoggingChannelRepository;
        }

        public async ValueTask<ITextChannel?> FindLogChannelAsync(IGuild guild)
        {
            var logChannel = await _memberLoggingChannelRepository.GetLogChannelForGuildAsync(guild);

            return logChannel != null ?
                (await guild.GetTextChannelsAsync()).FirstOrDefault(c => logChannel.ChannelId.Id == c.Id) :
                null;
        }
    }
}
