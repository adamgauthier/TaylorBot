using TaylorBot.Net.Commands.PageMessages;

namespace TaylorBot.Net.Commands.DiscordNet.PageMessages;

public class TextPageMessageRenderer(MessageTextEditor editor) : IPageMessageRenderer
{
    private int _currentPage = 1;

    public bool HasMultiplePages => editor.PageCount > 1;

    public MessageContent RenderNext()
    {
        _currentPage = _currentPage + 1 > editor.PageCount ? 1 : _currentPage + 1;
        return Render();
    }

    public MessageContent RenderPrevious()
    {
        _currentPage = _currentPage - 1 == 0 ? editor.PageCount : _currentPage - 1;
        return Render();
    }

    public MessageContent Render()
    {
        return editor.Edit(_currentPage);
    }
}
