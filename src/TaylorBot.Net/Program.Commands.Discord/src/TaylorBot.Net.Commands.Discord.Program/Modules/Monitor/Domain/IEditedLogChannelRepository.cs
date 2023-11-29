using Discord;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain
{
    public record EditedLog(SnowflakeId ChannelId);

    public interface IEditedLogChannelRepository
    {
        ValueTask AddOrUpdateEditedLogAsync(ITextChannel textChannel);
        ValueTask<EditedLog?> GetEditedLogForGuildAsync(IGuild guild);
        ValueTask RemoveEditedLogAsync(IGuild guild);
    }
}
