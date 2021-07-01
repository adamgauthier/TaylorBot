using System.Collections.Generic;

namespace TaylorBot.Net.Commands.PageMessages
{
    public class PageMessageResultRenderer
    {
        private int _currentPage = 1;

        private readonly IReadOnlyList<string> _pages;

        public bool HasMultiplePages => _pages.Count > 1;

        public PageMessageResultRenderer(IReadOnlyList<string> pages)
        {
            _pages = pages;
        }

        public MessageContent RenderNext()
        {
            _currentPage = _currentPage + 1 > _pages.Count ? 1 : _currentPage + 1;
            return Render();
        }

        public MessageContent RenderPrevious()
        {
            _currentPage = _currentPage - 1 == 0 ? _pages.Count : _currentPage - 1;
            return Render();
        }

        public MessageContent Render()
        {
            return new(_pages[_currentPage - 1]);
        }
    }
}
