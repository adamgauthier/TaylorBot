using Discord;
using System.Collections.Generic;

namespace TaylorBot.Net.Commands.PageMessages
{
    public class EmbedDescriptionTextEditor : IMessageEditor
    {
        private readonly EmbedBuilder _baseEmbed;
        private readonly IReadOnlyList<string> _pages;
        private readonly bool _hasPageFooter;

        public int PageCount => _pages.Count;

        public EmbedDescriptionTextEditor(EmbedBuilder baseEmbed, IReadOnlyList<string> pages, bool hasPageFooter)
        {
            _baseEmbed = baseEmbed;
            _pages = pages;
            _hasPageFooter = hasPageFooter;
        }

        public MessageContent Edit(int currentPage)
        {
            _baseEmbed.WithDescription(_pages[currentPage - 1]);

            if (_hasPageFooter)
            {
                _baseEmbed.WithFooter($"Page {currentPage}/{PageCount}");
            }

            return new(_baseEmbed.Build());
        }
    }
}
