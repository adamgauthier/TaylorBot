using System.Collections.Generic;

namespace TaylorBot.Net.Commands.PageMessages
{
    public class MessageTextEditor : IMessageEditor
    {
        private readonly IReadOnlyList<string> _pages;

        public int PageCount => _pages.Count;

        public MessageTextEditor(IReadOnlyList<string> pages)
        {
            _pages = pages;
        }

        public MessageContent Edit(int currentPage)
        {
            return new(_pages[currentPage - 1]);
        }
    }
}
