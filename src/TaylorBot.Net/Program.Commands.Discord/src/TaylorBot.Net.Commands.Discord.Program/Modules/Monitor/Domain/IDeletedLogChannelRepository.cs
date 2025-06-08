using Discord;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;

public record DeletedLog(SnowflakeId ChannelId);

public interface IDeletedLogChannelRepository
{
    ValueTask AddOrUpdateDeletedLogAsync(GuildTextChannel textChannel);
    ValueTask<DeletedLog?> GetDeletedLogForGuildAsync(IGuild guild);
    ValueTask RemoveDeletedLogAsync(CommandGuild guild);
}
