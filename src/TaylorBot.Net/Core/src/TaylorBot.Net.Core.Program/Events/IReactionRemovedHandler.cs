using Discord;
using Discord.WebSocket;

namespace TaylorBot.Net.Core.Program.Events;

public interface IReactionRemovedHandler
{
    ValueTask ReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction);
}
