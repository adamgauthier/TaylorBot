namespace TaylorBot.Net.Commands.PageMessages;

public class MessageTextEditor(IReadOnlyList<string> pages, string emptyText) : IMessageEditor
{
    public int PageCount => pages.Count;

    public MessageContent Edit(int currentPage)
    {
        if (PageCount > 0)
        {
            return new(pages[currentPage - 1]);
        }
        else
        {
            return new(emptyText);
        }
    }
}
