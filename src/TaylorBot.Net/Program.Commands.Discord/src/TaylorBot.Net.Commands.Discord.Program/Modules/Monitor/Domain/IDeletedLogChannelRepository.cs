using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain
{
    public record DeletedLog(SnowflakeId ChannelId);

    public interface IDeletedLogChannelRepository
    {
        ValueTask AddOrUpdateDeletedLogAsync(ITextChannel textChannel);
        ValueTask<DeletedLog?> GetDeletedLogForGuildAsync(IGuild guild);
        ValueTask RemoveDeletedLogAsync(IGuild guild);
    }
}
