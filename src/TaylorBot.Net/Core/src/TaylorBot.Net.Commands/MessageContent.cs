using Discord;
using System.Collections.Generic;

namespace TaylorBot.Net.Commands
{
    public record MessageContent(IReadOnlyList<Embed> Embeds, string Content = "")
    {
        public MessageContent(Embed Embed) : this(new[] { Embed }) { }
    }

    public enum ButtonStyle { Primary, Secondary, Success, Danger }

    public record Button(string Id, ButtonStyle Style, string Label, string? Emoji = null);

    public record MessageResponse(MessageContent Content, IReadOnlyList<Button>? Buttons = null)
    {
        public MessageResponse(Embed Embed) : this(new MessageContent(Embed)) { }
    }
}
