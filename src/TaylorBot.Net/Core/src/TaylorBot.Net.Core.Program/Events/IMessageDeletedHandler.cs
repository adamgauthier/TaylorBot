using Discord;

namespace TaylorBot.Net.Core.Program.Events
{
    public interface IMessageDeletedHandler
    {
        ValueTask MessageDeletedAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel);
    }
}
