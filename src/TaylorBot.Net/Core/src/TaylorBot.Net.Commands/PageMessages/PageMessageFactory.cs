using TaylorBot.Net.Commands.PostExecution;
using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands.PageMessages;

public record PageOptions(Guid Id, PageMessageResultRenderer Renderer, bool IsCancellable)
{
    public PageOptions(PageMessageResultRenderer Renderer, bool IsCancellable = false) : this(Guid.NewGuid(), Renderer, IsCancellable) { }

    public IList<CustomIdDataEntry> CustomIdData => [new("opt", $"{Id:N}")];
}

public class PageMessageFactory(PageOptionsInMemoryRepository repository)
{
    public MessageResult Create(PageOptions options)
    {
        List<ButtonResult> _buttons = [];

        if (options.Renderer.HasMultiplePages)
        {
            ButtonResult previousButton = new(
                new(
                    InteractionCustomId.Create(CustomIdNames.PageMessagePrevious, options.CustomIdData).RawId,
                    ButtonStyle.Primary, Label: "Previous", Emoji: "◀"),
                _ => throw new NotImplementedException()
            );

            ButtonResult nextButton = new(
                new(
                    InteractionCustomId.Create(CustomIdNames.PageMessageNext, options.CustomIdData).RawId,
                    ButtonStyle.Primary, Label: "Next", Emoji: "▶"),
                _ => throw new NotImplementedException()
            );

            _buttons.AddRange([previousButton, nextButton]);
        }

        if (options.IsCancellable)
        {
            // custom register to invalidate cache as well?
            ButtonResult cancelButton = new(
                new(
                    InteractionCustomId.Create(CustomIdNames.GenericMessageDelete).RawId,
                    ButtonStyle.Danger, Label: "Cancel", Emoji: "🗑"),
                _ => throw new NotImplementedException()
            );

            _buttons.Add(cancelButton);
        }

        repository.Register(options);
        return new(options.Renderer.Render(), new(_buttons, new PermanentButtonSettings()));
    }
}
