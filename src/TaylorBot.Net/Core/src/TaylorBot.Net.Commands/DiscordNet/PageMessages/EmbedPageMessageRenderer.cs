using Discord;

namespace TaylorBot.Net.Commands.DiscordNet.PageMessages;

public interface IEmbedPageEditor
{
    int PageCount { get; }
    EmbedBuilder Edit(EmbedBuilder embed, int currentPage);
}

public interface IPageMessageRenderer
{
    bool HasMultiplePages { get; }
    MessageContent RenderNext();
    MessageContent RenderPrevious();
    MessageContent Render();
}

public class EmbedPageMessageRenderer(IEmbedPageEditor editor, Func<EmbedBuilder> baseEmbedBuilder) : IPageMessageRenderer
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
        var edited = editor.Edit(baseEmbedBuilder(), _currentPage);

        if (edited.Footer == null && editor.PageCount > 0)
            edited.WithFooter($"Page {_currentPage}/{editor.PageCount}");

        return new(edited.Build());
    }
}
