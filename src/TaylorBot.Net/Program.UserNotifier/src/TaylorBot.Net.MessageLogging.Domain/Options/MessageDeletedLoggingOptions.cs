namespace TaylorBot.Net.MessageLogging.Domain.Options;

public class MessageDeletedLoggingOptions
{
    public string MessageReactionRemovedEmbedColorHex { get; set; } = null!;
    public string MessageDeletedEmbedColorHex { get; set; } = null!;
    public string MessageBulkDeletedEmbedColorHex { get; set; } = null!;
    public string MessageEditedEmbedColorHex { get; set; } = null!;
    public string? MessagePinnedEmbedColorHex { get; set; }
    public string? MessageUnpinnedEmbedColorHex { get; set; }
    public string? MessagePublishedEmbedColorHex { get; set; }
    public string? MessageEmbedsSuppressedEmbedColorHex { get; set; }
    public string? MessageEmbedsRestoredEmbedColorHex { get; set; }
    public string? MessageAttachmentsChangedEmbedColorHex { get; set; }
    public string? MessageThreadCreatedEmbedColorHex { get; set; }
    public bool UseRedisCache { get; set; }
}
