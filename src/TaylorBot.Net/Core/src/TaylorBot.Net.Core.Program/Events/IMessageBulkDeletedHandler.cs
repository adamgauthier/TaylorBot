using Discord;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageBulkDeletedHandler
    {
        ValueTask MessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> channel);
    }
}
