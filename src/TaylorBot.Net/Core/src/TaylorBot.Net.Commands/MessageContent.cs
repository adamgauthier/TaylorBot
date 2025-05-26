using Discord;
using TaylorBot.Net.Commands.PostExecution;

namespace TaylorBot.Net.Commands;

public record MessageContent(IReadOnlyList<Embed> Embeds, string Content = "", IReadOnlyList<Attachment>? Attachments = null)
{
    public MessageContent(Embed Embed, string Content = "") : this([Embed], Content) { }
    public MessageContent(string Content) : this([], Content) { }
}

public enum ButtonStyle { Primary, Secondary, Success, Danger }

public record Button(string Id, ButtonStyle Style, string Label, string? Emoji = null);

public record Attachment(Stream Stream, string Filename);

public record MessageResponse(MessageContent Content, IReadOnlyList<InteractionComponent>? Components = null, bool IsPrivate = false)
{
    public MessageResponse(MessageContent Content, IReadOnlyList<Button> Buttons, bool IsPrivate = false) : this(Content, ToInteractionComponents(Buttons), IsPrivate) { }

    public MessageResponse(Embed Embed) : this(new MessageContent(Embed)) { }

    public static IReadOnlyList<InteractionComponent> ToInteractionComponents(IReadOnlyList<Button> buttons)
    {
        if (buttons.Count == 0)
        {
            return [];
        }

        return [
            InteractionComponent.CreateActionRow([.. buttons.Select(b =>
                InteractionComponent.CreateButton(
                    style: ToInteractionStyle(b.Style),
                    label: b.Label,
                    custom_id: b.Id,
                    emoji: b.Emoji != null ? new(name: b.Emoji) : null
                )
            )])
        ];
    }

    private static InteractionButtonStyle ToInteractionStyle(ButtonStyle style)
    {
        return style switch
        {
            ButtonStyle.Primary => InteractionButtonStyle.Primary,
            ButtonStyle.Secondary => InteractionButtonStyle.Secondary,
            ButtonStyle.Success => InteractionButtonStyle.Success,
            ButtonStyle.Danger => InteractionButtonStyle.Danger,
            _ => throw new ArgumentOutOfRangeException(nameof(style)),
        };
    }

    public static MessageResponse CreatePrompt(MessageContent content, InteractionCustomId confirmButtonId)
    {
        return new MessageResponse(content,
        [
            new Button(confirmButtonId.RawId, ButtonStyle.Success, Label: "Confirm"),
            new Button(InteractionCustomId.Create(CustomIdNames.GenericPromptCancel).RawId, ButtonStyle.Danger, Label: "Cancel"),
        ]);
    }
}
