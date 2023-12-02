using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;

public record ModLog(SnowflakeId ChannelId);

public interface IModLogChannelRepository
{
    ValueTask AddOrUpdateModLogAsync(ITextChannel textChannel);
    ValueTask RemoveModLogAsync(IGuild guild);
    ValueTask<ModLog?> GetModLogForGuildAsync(IGuild guild);
}
