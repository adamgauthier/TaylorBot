using Discord;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PostExecution;
using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands;

public interface ICommandResult { }


public record EmbedResult(Embed Embed) : ICommandResult;

public record EmptyResult() : ICommandResult;

public record RateLimitedResult(string FriendlyLimitName, long Uses, uint Limit) : ICommandResult;

public record PageMessageResult(PageMessage PageMessage) : ICommandResult;


public record MessageResult(MessageContent Content, ButtonConfig? Buttons = null) : ICommandResult
{
    public static MessageResult CreatePrompt(MessageContent content, InteractionCustomId confirmButtonId)
    {
        return new MessageResult(content, new([
            new ButtonResult(
                new Button(confirmButtonId.RawId, ButtonStyle.Success, Label: "Confirm"),
                _ => throw new NotImplementedException()
            ),
            new ButtonResult(
                new Button(InteractionCustomId.Create(CustomIdNames.GenericPromptCancel).RawId, ButtonStyle.Danger, Label: "Cancel"),
                _ => throw new NotImplementedException()
            ),
        ], new PermanentButtonSettings()));
    }

    public record ButtonConfig(IReadOnlyList<ButtonResult> Buttons, IButtonSettings Settings)
    {
        public ButtonConfig(IReadOnlyList<ButtonResult> Buttons) : this(Buttons, new TemporaryButtonSettings()) { }
    }

    public interface IButtonSettings { }
    public record TemporaryButtonSettings(TimeSpan? ListenToClicksFor = null, Func<ValueTask<MessageResult>>? OnEnded = null) : IButtonSettings;
    public record PermanentButtonSettings() : IButtonSettings;

    public record ButtonResult(Button Button, Func<string, ValueTask<IButtonClickResult>> OnClick, bool AllowNonAuthor = false);

    public interface IButtonClickResult { }
    public record UpdateMessage(MessageResult NewMessage) : IButtonClickResult;
    public record UpdateMessageContent(MessageContent Content, UpdateMessageContent.FollowupResponse? Response = null) : IButtonClickResult
    {
        public record FollowupResponse(MessageResult Message, bool IsPrivate);
    }
    public record DeleteMessage() : IButtonClickResult;
    public record IgnoreClick() : IButtonClickResult;
}

public enum TextInputStyle { Short, Paragraph }

public record TextInput(string Id, TextInputStyle Style, string Label, bool Required = true, int? MinLength = null, int? MaxLength = null);

public record CreateModalResult(string Id, string Title, IReadOnlyList<TextInput> TextInputs, Func<ModalSubmit, ValueTask<MessageResult>>? SubmitAction, bool IsPrivateResponse) : ICommandResult;
