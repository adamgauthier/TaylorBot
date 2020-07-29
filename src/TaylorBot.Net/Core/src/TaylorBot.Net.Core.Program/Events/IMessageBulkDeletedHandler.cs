using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageBulkDeletedHandler
    {
        ValueTask MessageBulkDeletedAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, ISocketMessageChannel channel);
    }
}
