using Discord;
using System;
using System.Collections.Generic;

namespace TaylorBot.Net.Commands.PageMessages
{
    public class EmbedDescriptionPageMessageRenderer
    {
        private int _currentPage = 1;

        private readonly IReadOnlyList<string> _pages;
        private readonly Func<EmbedBuilder> _baseEmbedBuilder;

        public int PageCount => _pages.Count;

        public EmbedDescriptionPageMessageRenderer(IReadOnlyList<string> pages, Func<EmbedBuilder> baseEmbedBuilder)
        {
            _pages = pages;
            _baseEmbedBuilder = baseEmbedBuilder;
        }

        public Embed RenderNext()
        {
            _currentPage = _currentPage + 1 > PageCount ? 1 : _currentPage + 1;
            return Render();
        }

        public Embed RenderPrevious()
        {
            _currentPage = _currentPage - 1 == 0 ? PageCount : _currentPage - 1;
            return Render();
        }

        public Embed Render()
        {
            return _baseEmbedBuilder()
                .WithDescription(_pages[_currentPage - 1])
                .WithFooter($"Page {_currentPage}/{PageCount}")
            .Build();
        }
    }
}
