using Discord;
using TaylorBot.Net.MemberLogging.Domain.DiscordEmbed;

namespace TaylorBot.Net.MemberLogging.Domain
{
    public class GuildMemberLeftLoggerService
    {
        private readonly MemberLogChannelFinder _memberLogChannelFinder;
        private readonly GuildMemberLeftEmbedFactory _guildMemberLeftEmbedFactory;

        public GuildMemberLeftLoggerService(MemberLogChannelFinder memberLogChannelFinder, GuildMemberLeftEmbedFactory guildMemberLeftEmbedFactory)
        {
            _memberLogChannelFinder = memberLogChannelFinder;
            _guildMemberLeftEmbedFactory = guildMemberLeftEmbedFactory;
        }

        public async Task OnGuildMemberLeftAsync(IGuild guild, IUser user)
        {
            var logTextChannel = await _memberLogChannelFinder.FindLogChannelAsync(guild);

            if (logTextChannel != null)
                await logTextChannel.SendMessageAsync(embed: _guildMemberLeftEmbedFactory.CreateMemberLeft(user));
        }
    }
}
