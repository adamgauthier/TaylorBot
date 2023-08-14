namespace TaylorBot.Net.Commands.PageMessages;

public interface IMessageEditor
{
    int PageCount { get; }
    MessageContent Edit(int currentPage);
}

public class PageMessageResultRenderer
{
    private int _currentPage = 1;

    private readonly IMessageEditor _editor;

    public bool HasMultiplePages => _editor.PageCount > 1;

    public PageMessageResultRenderer(IMessageEditor editor)
    {
        _editor = editor;
    }

    public MessageContent RenderNext()
    {
        _currentPage = _currentPage + 1 > _editor.PageCount ? 1 : _currentPage + 1;
        return Render();
    }

    public MessageContent RenderPrevious()
    {
        _currentPage = _currentPage - 1 == 0 ? _editor.PageCount : _currentPage - 1;
        return Render();
    }

    public MessageContent Render()
    {
        return _editor.Edit(_currentPage);
    }
}
