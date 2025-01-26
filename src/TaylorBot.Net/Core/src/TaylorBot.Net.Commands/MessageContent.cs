using Discord;

namespace TaylorBot.Net.Commands;

public record MessageContent(IReadOnlyList<Embed> Embeds, string Content = "", IReadOnlyList<Attachment>? Attachments = null)
{
    public MessageContent(Embed Embed, string Content = "") : this([Embed], Content) { }
    public MessageContent(string Content) : this([], Content) { }
}

public enum ButtonStyle { Primary, Secondary, Success, Danger }

public record Button(string Id, ButtonStyle Style, string Label, string? Emoji = null);

public record Attachment(Stream Stream, string Filename);

public record MessageResponse(MessageContent Content, IReadOnlyList<Button>? Buttons = null, bool IsPrivate = false)
{
    public MessageResponse(Embed Embed) : this(new MessageContent(Embed)) { }
}
