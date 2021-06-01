using Discord;
using System.Collections.Generic;

namespace TaylorBot.Net.Commands
{
    public record MessageResponse(IReadOnlyList<Embed> Embeds, string Content = "")
    {
        public MessageResponse(Embed Embed) : this(new[] { Embed }) { }
    }
}
