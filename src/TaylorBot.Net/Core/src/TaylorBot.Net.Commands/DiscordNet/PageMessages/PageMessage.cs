using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Commands.DiscordNet.PageMessages;

public record PageMessageOptions(IPageMessageRenderer Renderer, bool Cancellable = false, List<Emoji>? AdditionalReacts = null);

public class PageMessage(PageMessageOptions options)
{
    public async ValueTask<SentPageMessage> SendAsync(IUser commandUser, IMessageChannel channel)
    {
        var rendered = options.Renderer.Render();
        var message = await channel.SendMessageAsync(
            text: string.IsNullOrWhiteSpace(rendered.Content) ? null : rendered.Content,
            embed: rendered.Embeds.Count > 0 ? rendered.Embeds[0] : null);
        return new SentPageMessage(commandUser, message, options);
    }
}

public class SentPageMessage(IUser commandUser, IUserMessage sentMessage, PageMessageOptions options)
{
    private static readonly Emoji PreviousEmoji = new("◀");
    private static readonly Emoji NextEmoji = new("▶");
    private static readonly Emoji CancelEmoji = new("❌");

    private DateTimeOffset? _lastInteractionAt = null;
    private Timer? _unsubscribeTimer = null;

    public async ValueTask SendReactionsAsync(PageMessageReactionsHandler pageMessageReactionsHandler, ILogger logger)
    {
        var interactiveEmotes = new List<Emoji>();
        if (options.Renderer.HasMultiplePages)
        {
            interactiveEmotes.AddRange(new[] { PreviousEmoji, NextEmoji });
        }
        if (options.Cancellable)
        {
            interactiveEmotes.Add(CancelEmoji);
        }

        var allEmotes = interactiveEmotes.Concat(options.AdditionalReacts ?? []).ToList();

        if (allEmotes.Count > 0)
        {
            if (interactiveEmotes.Count > 0)
            {
                pageMessageReactionsHandler.OnReact += OnReactAsync;
            }

            try
            {
                await sentMessage.AddReactionsAsync(allEmotes);
            }
            catch
            {
                logger.LogWarning("Could not add reactions for page message {Message} by {User}.", sentMessage.FormatLog(), commandUser.FormatLog());
            }
            finally
            {
                if (interactiveEmotes.Count > 0)
                {
                    _unsubscribeTimer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
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
    }

    private async Task OnReactAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (message.Id == sentMessage.Id && reaction.UserId == commandUser.Id)
        {
            if (reaction.Emote.Equals(PreviousEmoji))
            {
                _lastInteractionAt = DateTimeOffset.Now;
                await sentMessage.ModifyAsync(m =>
                {
                    var next = options.Renderer.RenderNext();
                    if (!string.IsNullOrWhiteSpace(next.Content))
                    {
                        m.Content = next.Content;
                    }
                    if (next.Embeds.Count > 0)
                    {
                        m.Embed = next.Embeds[0];
                    }
                });
            }
            else if (reaction.Emote.Equals(NextEmoji))
            {
                _lastInteractionAt = DateTimeOffset.Now;
                await sentMessage.ModifyAsync(m =>
                {
                    var previous = options.Renderer.RenderPrevious();
                    if (!string.IsNullOrWhiteSpace(previous.Content))
                    {
                        m.Content = previous.Content;
                    }
                    if (previous.Embeds.Count > 0)
                    {
                        m.Embed = previous.Embeds[0];
                    }
                });
            }
            else if (reaction.Emote.Equals(CancelEmoji) && options.Cancellable)
            {
                await sentMessage.DeleteAsync();
            }
        }
    }
}
