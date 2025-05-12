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
        List<Button> buttons = [];

        if (options.Renderer.HasMultiplePages)
        {
            Button previousButton = new(
                InteractionCustomId.Create(CustomIdNames.PageMessagePrevious, options.CustomIdData).RawId,
                ButtonStyle.Primary, Label: "Previous", Emoji: "◀"
            );

            Button nextButton = new(
                InteractionCustomId.Create(CustomIdNames.PageMessageNext, options.CustomIdData).RawId,
                ButtonStyle.Primary, Label: "Next", Emoji: "▶"
            );

            buttons.AddRange([previousButton, nextButton]);
        }

        if (options.IsCancellable)
        {
            buttons.Add(CreateCancelButton(options.CustomIdData));
        }

        repository.Register(options);
        return new(new(options.Renderer.Render(), buttons));
    }

    public static Button CreateCancelButton(IList<CustomIdDataEntry> customIdData)
    {
        return new(
            InteractionCustomId.Create(CustomIdNames.PageMessageCancel, customIdData).RawId,
            ButtonStyle.Danger, Label: "Cancel", Emoji: "🗑"
        );
    }
}
