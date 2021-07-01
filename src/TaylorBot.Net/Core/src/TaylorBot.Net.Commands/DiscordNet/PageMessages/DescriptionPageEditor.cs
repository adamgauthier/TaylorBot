using Discord;
using System.Collections.Generic;

namespace TaylorBot.Net.Commands.DiscordNet.PageMessages
{
    public class DescriptionPageEditor : IEmbedPageEditor
    {
        private readonly IReadOnlyList<string> _pages;

        public int PageCount => _pages.Count;

        public DescriptionPageEditor(IReadOnlyList<string> pages)
        {
            _pages = pages;
        }

        public EmbedBuilder Edit(EmbedBuilder embed, int currentPage)
        {
            return embed.WithDescription(_pages[currentPage - 1]);
        }
    }
}
