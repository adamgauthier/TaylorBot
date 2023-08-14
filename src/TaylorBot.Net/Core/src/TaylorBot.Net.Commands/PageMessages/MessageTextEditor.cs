using System.Collections.Generic;

namespace TaylorBot.Net.Commands.PageMessages;

public class MessageTextEditor : IMessageEditor
{
    private readonly IReadOnlyList<string> _pages;
    private readonly string _emptyText;

    public int PageCount => _pages.Count;

    public MessageTextEditor(IReadOnlyList<string> pages, string emptyText)
    {
        _pages = pages;
        _emptyText = emptyText;
    }

    public MessageContent Edit(int currentPage)
    {
        if (PageCount > 0)
        {
            return new(_pages[currentPage - 1]);
        }
        else
        {
            return new(_emptyText);
        }
    }
}
