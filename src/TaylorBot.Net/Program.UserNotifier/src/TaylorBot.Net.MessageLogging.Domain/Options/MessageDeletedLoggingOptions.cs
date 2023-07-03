namespace TaylorBot.Net.MessageLogging.Domain.Options;

public class MessageDeletedLoggingOptions
{
    public string MessageReactionRemovedEmbedColorHex { get; set; } = null!;
    public string MessageDeletedEmbedColorHex { get; set; } = null!;
    public string MessageBulkDeletedEmbedColorHex { get; set; } = null!;
    public string MessageEditedEmbedColorHex { get; set; } = null!;
    public bool UseRedisCache { get; set; }
}
