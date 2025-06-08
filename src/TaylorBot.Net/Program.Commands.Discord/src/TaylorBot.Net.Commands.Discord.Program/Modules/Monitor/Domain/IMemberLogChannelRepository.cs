using Discord;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;

public record MemberLog(SnowflakeId ChannelId);

public interface IMemberLogChannelRepository
{
    ValueTask AddOrUpdateMemberLogAsync(GuildTextChannel textChannel);
    ValueTask<MemberLog?> GetMemberLogForGuildAsync(IGuild guild);
    ValueTask RemoveMemberLogAsync(CommandGuild guild);
}
