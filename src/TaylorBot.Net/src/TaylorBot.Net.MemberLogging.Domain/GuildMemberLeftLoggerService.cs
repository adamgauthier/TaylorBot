using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain
{
    public class GuildMemberLeftLoggerService
    {
        private readonly MemberLogChannelFinder memberLogChannelFinder;
        private readonly GuildMemberLeftEmbedFactory guildMemberLeftEmbedFactory;

        public GuildMemberLeftLoggerService(
            MemberLogChannelFinder memberLogChannelFinder,
            GuildMemberLeftEmbedFactory guildMemberLeftEmbedFactory)
        {
            this.memberLogChannelFinder = memberLogChannelFinder;
            this.guildMemberLeftEmbedFactory = guildMemberLeftEmbedFactory;
        }

        public async Task OnGuildMemberLeftAsync(IGuildUser guildUser)
        {
            var logTextChannel = await memberLogChannelFinder.FindLogChannelAsync(guildUser.Guild);

            if (logTextChannel != null)
                await logTextChannel.SendMessageAsync(embed: guildMemberLeftEmbedFactory.CreateMemberLeft(guildUser));
        }
    }
}
