using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands.PageMessages;

public record PageOptions(PageMessageResultRenderer Renderer, bool IsCancellable = false);

public class PageMessageResultBuilder
{
    private readonly PageOptions _options;
    private readonly List<ButtonResult> _buttons;

    public PageMessageResultBuilder(PageOptions options)
    {
        _options = options;

        _buttons = [];

        if (_options.Renderer.HasMultiplePages)
        {
            var previousButton = new ButtonResult(
                new Button(Id: "previous", ButtonStyle.Primary, Label: "Previous", Emoji: "◀"),
                PreviousAsync
            );

            var nextButton = new ButtonResult(
                new Button(Id: "next", ButtonStyle.Primary, Label: "Next", Emoji: "▶"),
                NextAsync
            );

            _buttons.AddRange([previousButton, nextButton]);
        }

        if (_options.IsCancellable)
        {
            var cancelButton = new ButtonResult(
                new Button(Id: "cancel", ButtonStyle.Danger, Label: "Cancel", Emoji: "🗑"),
                _ => new(new DeleteMessage())
            );

            _buttons.Add(cancelButton);
        }
    }

    private ValueTask<IButtonClickResult> PreviousAsync(string userId)
    {
        var content = _options.Renderer.RenderPrevious();
        return new(new UpdateMessage(new(content, new(_buttons))));
    }

    private ValueTask<IButtonClickResult> NextAsync(string userId)
    {
        var content = _options.Renderer.RenderNext();
        return new(new UpdateMessage(new(content, new(_buttons))));
    }

    public MessageResult Build()
    {
        return new(_options.Renderer.Render(), new(_buttons));
    }
}
