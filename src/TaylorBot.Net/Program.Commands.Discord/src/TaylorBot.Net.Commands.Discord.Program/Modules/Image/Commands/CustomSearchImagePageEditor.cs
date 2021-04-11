using Discord;
using System.Collections.Generic;
using TaylorBot.Net.Commands.Discord.Program.Modules.Image.Domain;
using TaylorBot.Net.Commands.PageMessages;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Image.Commands
{
    public class CustomSearchImagePageEditor : IEmbedPageEditor
    {
        private readonly IReadOnlyList<ImageResult> _pages;

        public int PageCount => _pages.Count;

        public CustomSearchImagePageEditor(IReadOnlyList<ImageResult> pages)
        {
            _pages = pages;
        }

        public EmbedBuilder Edit(EmbedBuilder embed, int currentPage)
        {
            var page = _pages[currentPage - 1];

            return embed
                .WithTitle(page.Title)
                .WithUrl(page.PageUrl)
                .WithImageUrl(page.ImageUrl);
        }
    }
}
