using Discord;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;

public record ModLog(SnowflakeId ChannelId);

public interface IModLogChannelRepository
{
    ValueTask AddOrUpdateModLogAsync(GuildTextChannel textChannel);
    ValueTask RemoveModLogAsync(IGuild guild);
    ValueTask<ModLog?> GetModLogForGuildAsync(IGuild guild);
}
