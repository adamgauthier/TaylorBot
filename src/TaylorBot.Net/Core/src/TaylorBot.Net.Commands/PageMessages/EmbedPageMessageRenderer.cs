using Discord;
using System;

namespace TaylorBot.Net.Commands.PageMessages
{
    public interface IEmbedPageEditor
    {
        int PageCount { get; }
        EmbedBuilder Edit(EmbedBuilder embed, int currentPage);
    }

    public class EmbedPageMessageRenderer
    {
        private int _currentPage = 1;

        private readonly IEmbedPageEditor _editor;
        private readonly Func<EmbedBuilder> _baseEmbedBuilder;

        public bool HasMultiplePages => _editor.PageCount > 1;

        public EmbedPageMessageRenderer(IEmbedPageEditor editor, Func<EmbedBuilder> baseEmbedBuilder)
        {
            _editor = editor;
            _baseEmbedBuilder = baseEmbedBuilder;
        }

        public Embed RenderNext()
        {
            _currentPage = _currentPage + 1 > _editor.PageCount ? 1 : _currentPage + 1;
            return Render();
        }

        public Embed RenderPrevious()
        {
            _currentPage = _currentPage - 1 == 0 ? _editor.PageCount : _currentPage - 1;
            return Render();
        }

        public Embed Render()
        {
            var edited = _editor.Edit(_baseEmbedBuilder(), _currentPage);

            if (edited.Footer == null)
                edited.WithFooter($"Page {_currentPage}/{_editor.PageCount}");

            return edited.Build();
        }
    }
}
