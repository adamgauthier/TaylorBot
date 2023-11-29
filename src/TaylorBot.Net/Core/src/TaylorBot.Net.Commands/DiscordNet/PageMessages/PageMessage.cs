using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet.PageMessages;

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

public class SentPageMessage(IUser commandUser, IUserMessage message, PageMessageOptions options)
{
    private static readonly Emoji PreviousEmoji = new("◀");
    private static readonly Emoji NextEmoji = new("▶");
    private static readonly Emoji CancelEmoji = new("❌");
    private readonly IUserMessage _message = message;
    private DateTimeOffset? _lastInteractionAt = null;
    private Timer? _unsubscribeTimer = null;

    public async ValueTask SendReactionsAsync(PageMessageReactionsHandler pageMessageReactionsHandler, ILogger logger)
    {
        var emotes = new List<Emoji>();
        if (options.Renderer.HasMultiplePages)
        {
            emotes.AddRange(new[] { PreviousEmoji, NextEmoji });
        }
        if (options.Cancellable)
        {
            emotes.Add(CancelEmoji);
        }

        if (emotes.Count > 0)
        {
            pageMessageReactionsHandler.OnReact += OnReactAsync;

            try
            {
                await _message.AddReactionsAsync(emotes.ToArray());
            }
            catch
            {
                logger.LogWarning("Could not add reactions for page message {Message} by {User}.", _message.FormatLog(), commandUser.FormatLog());
            }
            finally
            {
                _unsubscribeTimer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
                void TimerCallback(object? state)
                {
                    if (_lastInteractionAt == null ||
                        (DateTimeOffset.Now - _lastInteractionAt.Value).TotalSeconds > 30)
                    {
                        pageMessageReactionsHandler.OnReact -= OnReactAsync;
                        _unsubscribeTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
            }
        }
    }

    private async Task OnReactAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (message.Id == _message.Id && reaction.UserId == commandUser.Id)
        {
            if (reaction.Emote.Equals(PreviousEmoji))
            {
                _lastInteractionAt = DateTimeOffset.Now;
                await _message.ModifyAsync(m =>
                    m.Embed = options.Renderer.RenderNext()
                );
            }
            else if (reaction.Emote.Equals(NextEmoji))
            {
                _lastInteractionAt = DateTimeOffset.Now;
                await _message.ModifyAsync(m =>
                    m.Embed = options.Renderer.RenderPrevious()
                );
            }
            else if (reaction.Emote.Equals(CancelEmoji) && options.Cancellable)
            {
                await _message.DeleteAsync();
            }
        }
    }
}
