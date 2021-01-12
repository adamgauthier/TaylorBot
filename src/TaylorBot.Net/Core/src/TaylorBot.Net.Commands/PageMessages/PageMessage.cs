using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.PageMessages
{
    public class PageMessage
    {
        private readonly EmbedDescriptionPageMessageRenderer _pageMessageRenderer;

        public PageMessage(EmbedDescriptionPageMessageRenderer pageMessageRenderer)
        {
            _pageMessageRenderer = pageMessageRenderer;
        }

        public async ValueTask<SentPageMessage> SendAsync(IUser commandUser, IMessageChannel channel)
        {
            var message = await channel.SendMessageAsync(embed: _pageMessageRenderer.Render());
            return new SentPageMessage(commandUser, message, _pageMessageRenderer);
        }
    }

    public class SentPageMessage
    {
        private static readonly Emoji PreviousEmoji = new Emoji("◀");
        private static readonly Emoji NextEmoji = new Emoji("▶");

        private readonly IUser _commandUser;
        private readonly IUserMessage _message;
        private readonly EmbedDescriptionPageMessageRenderer _pageMessageRenderer;

        private DateTimeOffset? _lastInteractionAt = null;
        private Timer? _unsubscribeTimer = null;

        public SentPageMessage(IUser commandUser, IUserMessage message, EmbedDescriptionPageMessageRenderer pageMessageRenderer)
        {
            _commandUser = commandUser;
            _message = message;
            _pageMessageRenderer = pageMessageRenderer;
        }

        public async ValueTask SendReactionsAsync(PageMessageReactionsHandler pageMessageReactionsHandler, ILogger logger)
        {
            if (_pageMessageRenderer.PageCount > 1)
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

                try
                {
                    await _message.AddReactionsAsync(new[] { PreviousEmoji, NextEmoji });
                }
                catch
                {
                    logger.LogWarning(LogString.From($"Could not add reactions for page message {_message.FormatLog()} by {_commandUser.FormatLog()}."));
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
                        m.Embed = _pageMessageRenderer.RenderNext()
                    );
                }
                else if (reaction.Emote.Equals(NextEmoji))
                {
                    _lastInteractionAt = DateTimeOffset.Now;
                    await _message.ModifyAsync(m =>
                        m.Embed = _pageMessageRenderer.RenderPrevious()
                    );
                }
            }
        }
    }
}
