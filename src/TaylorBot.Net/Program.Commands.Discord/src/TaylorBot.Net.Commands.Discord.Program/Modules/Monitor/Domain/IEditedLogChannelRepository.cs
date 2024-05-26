using Discord;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;

public record EditedLog(SnowflakeId ChannelId);

public interface IEditedLogChannelRepository
{
    ValueTask AddOrUpdateEditedLogAsync(GuildTextChannel textChannel);
    ValueTask<EditedLog?> GetEditedLogForGuildAsync(IGuild guild);
    ValueTask RemoveEditedLogAsync(IGuild guild);
}
