using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.PageMessages
{
    public record PageMessageOptions(EmbedPageMessageRenderer Renderer, bool Cancellable = false);

    public class PageMessage
    {
        private readonly PageMessageOptions _options;

        public PageMessage(PageMessageOptions options)
        {
            _options = options;
        }

        public async ValueTask<SentPageMessage> SendAsync(IUser commandUser, IMessageChannel channel)
        {
            var message = await channel.SendMessageAsync(embed: _options.Renderer.Render());
            return new SentPageMessage(commandUser, message, _options);
        }
    }

    public class SentPageMessage
    {
        private static readonly Emoji PreviousEmoji = new("◀");
        private static readonly Emoji NextEmoji = new("▶");
        private static readonly Emoji CancelEmoji = new("❌");

        private readonly IUser _commandUser;
        private readonly IUserMessage _message;
        private readonly PageMessageOptions _options;

        private DateTimeOffset? _lastInteractionAt = null;
        private Timer? _unsubscribeTimer = null;

        public SentPageMessage(IUser commandUser, IUserMessage message, PageMessageOptions options)
        {
            _commandUser = commandUser;
            _message = message;
            _options = options;
        }

        public async ValueTask SendReactionsAsync(PageMessageReactionsHandler pageMessageReactionsHandler, ILogger logger)
        {
            if (_options.Renderer.HasMultiplePages)
            {
                pageMessageReactionsHandler.OnReact += OnReactAsync;

                _unsubscribeTimer = new Timer(interval: TimeSpan.FromSeconds(30).TotalMilliseconds);
                _unsubscribeTimer.Elapsed += (source, arguments) =>
                {
                    if (_lastInteractionAt == null ||
                        (DateTimeOffset.Now - _lastInteractionAt.Value).TotalSeconds > 30)
                    {
                        pageMessageReactionsHandler.OnReact -= OnReactAsync;
                        ((Timer)source).Stop();
                    }
                };

                var emotes = new List<Emoji> { PreviousEmoji, NextEmoji };

                if (_options.Cancellable)
                    emotes.Add(CancelEmoji);

                try
                {
                    await _message.AddReactionsAsync(emotes.ToArray());
                }
                catch
                {
                    logger.LogWarning($"Could not add reactions for page message {_message.FormatLog()} by {_commandUser.FormatLog()}.");
                }
                finally
                {
                    _unsubscribeTimer.Start();
                }
            }
        }

        private async Task OnReactAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (message.Id == _message.Id && reaction.UserId == _commandUser.Id)
            {
                if (reaction.Emote.Equals(PreviousEmoji))
                {
                    _lastInteractionAt = DateTimeOffset.Now;
                    await _message.ModifyAsync(m =>
                        m.Embed = _options.Renderer.RenderNext()
                    );
                }
                else if (reaction.Emote.Equals(NextEmoji))
                {
                    _lastInteractionAt = DateTimeOffset.Now;
                    await _message.ModifyAsync(m =>
                        m.Embed = _options.Renderer.RenderPrevious()
                    );
                }
                else if (reaction.Emote.Equals(CancelEmoji) && _options.Cancellable)
                {
                    await _message.DeleteAsync();
                }
            }
        }
    }
}
