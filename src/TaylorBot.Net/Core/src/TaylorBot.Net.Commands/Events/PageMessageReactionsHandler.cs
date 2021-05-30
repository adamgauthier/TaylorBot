using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Events;
using TaylorBot.Net.Core.Program.Events;

namespace TaylorBot.Net.Commands.Events
{
    public class PageMessageReactionsHandler : IReactionAddedHandler, IReactionRemovedHandler
    {
        private readonly AsyncEvent<Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task>> _onReactEvent = new();

        public event Func<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction, Task> OnReact
        {
            add { _onReactEvent.Add(value); }
            remove { _onReactEvent.Remove(value); }
        }

        public async ValueTask ReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _onReactEvent.InvokeAsync(message, channel, reaction);
        }

        public async ValueTask ReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _onReactEvent.InvokeAsync(message, channel, reaction);
        }
    }
}
