using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.PageMessages
{
    public record PageOptions(PageMessageResultRenderer Renderer, bool IsCancellable = false);

    public class PageMessageResultBuilder
    {
        private readonly PageOptions _options;
        private readonly List<MessageResult.ButtonResult> _buttons;

        public PageMessageResultBuilder(PageOptions options)
        {
            _options = options;

            _buttons = new();

            if (_options.Renderer.HasMultiplePages)
            {
                var previousButton = new MessageResult.ButtonResult(
                    new Button(Id: "previous", ButtonStyle.Primary, Label: "Previous", Emoji: "◀"),
                    PreviousAsync
                );

                var nextButton = new MessageResult.ButtonResult(
                    new Button(Id: "next", ButtonStyle.Primary, Label: "Next", Emoji: "▶"),
                    NextAsync
                );

                _buttons.AddRange(new[] { previousButton, nextButton });
            }

            if (_options.IsCancellable)
            {
                var cancelButton = new MessageResult.ButtonResult(
                    new Button(Id: "cancel", ButtonStyle.Danger, Label: "Cancel", Emoji: "🗑"),
                    () => new((MessageResult?)null)
                );

                _buttons.Add(cancelButton);
            }
        }

        private ValueTask<MessageResult?> PreviousAsync()
        {
            var content = _options.Renderer.RenderPrevious();
            return new(new MessageResult(content, _buttons));
        }

        private ValueTask<MessageResult?> NextAsync()
        {
            var content = _options.Renderer.RenderNext();
            return new(new MessageResult(content, _buttons));
        }

        public MessageResult Build()
        {
            return new(_options.Renderer.Render(), _buttons);
        }
    }
}
