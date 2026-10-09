using Discord;

namespace TaylorBot.Net.MessageLogging.Domain;

public enum MessageUpdateAction
{
    Edited,
    Pinned,
    Unpinned,
    Published,
    EmbedsSuppressed,
    EmbedsRestored,
    AttachmentsChanged,
    ThreadCreated,
    PublishedSourceDeleted,
}

public record MessageUpdate(TaylorBotCachedMessageData? Before, TaylorBotCachedMessageData After, IReadOnlyList<MessageUpdateAction> Actions)
{
    public bool ContentChanged => Before?.Content != null && Before.Content != After.Content;
    public bool AttachmentsChanged => Before?.Attachments != null &&
        !Before.Attachments.Select(a => a.Id).Order().SequenceEqual(After.Attachments!.Select(a => a.Id).Order());

    public static MessageUpdate Compare(ICachedMessageData? cachedData, IMessage message)
    {
        var before = cachedData switch
        {
            DiscordNetCachedMessageData discordNet => TaylorBotCachedMessageData.FromMessage(discordNet.Message),
            TaylorBotCachedMessageData taylorBot => taylorBot,
            _ => null,
        };
        var after = TaylorBotCachedMessageData.FromMessage(message);
        List<MessageUpdateAction> actions = [];
        MessageUpdate update = new(before, after, actions);

        if (before?.IsPinned is bool wasPinned && wasPinned != after.IsPinned)
        {
            actions.Add(after.IsPinned == true ? MessageUpdateAction.Pinned : MessageUpdateAction.Unpinned);
        }
        if (before?.IsPublished == false && after.IsPublished == true)
        {
            actions.Add(MessageUpdateAction.Published);
        }
        if (before?.EmbedsSuppressed is bool wereSuppressed && after.EmbedsSuppressed is bool suppressed && wereSuppressed != suppressed)
        {
            actions.Add(suppressed ? MessageUpdateAction.EmbedsSuppressed : MessageUpdateAction.EmbedsRestored);
        }
        if (before?.HasThread == false && after.HasThread == true)
        {
            actions.Add(MessageUpdateAction.ThreadCreated);
        }
        if (before?.SourceMessageDeleted == false && after.SourceMessageDeleted == true &&
            (message.Flags & MessageFlags.IsCrosspost) == MessageFlags.IsCrosspost)
        {
            actions.Add(MessageUpdateAction.PublishedSourceDeleted);
        }
        if (update.ContentChanged)
        {
            actions.Add(MessageUpdateAction.Edited);
        }
        if (update.AttachmentsChanged)
        {
            actions.Add(MessageUpdateAction.AttachmentsChanged);
        }
        if (actions.Count == 0 && (before?.Content == null || before.IsPinned == null || before.IsPublished == null ||
            before.EmbedsSuppressed == null || before.HasThread == null || before.Attachments == null || before.SourceMessageDeleted == null))
        {
            actions.Add(MessageUpdateAction.Edited);
        }

        return update;
    }
}
