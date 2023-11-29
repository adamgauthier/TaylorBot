using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain
{
    public record MemberLog(SnowflakeId ChannelId);

    public interface IMemberLogChannelRepository
    {
        ValueTask AddOrUpdateMemberLogAsync(ITextChannel textChannel);
        ValueTask<MemberLog?> GetMemberLogForGuildAsync(IGuild guild);
        ValueTask RemoveMemberLogAsync(IGuild guild);
    }
}
