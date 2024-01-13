using Discord;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;
using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands;

public interface ICommandResult { }


public record EmbedResult(Embed Embed, bool PrefixCommandReply = false) : ICommandResult;

public record EmptyResult() : ICommandResult;

public record RateLimitedResult(string FriendlyLimitName, long Uses, uint Limit) : ICommandResult;

public record PageMessageResult(PageMessage PageMessage) : ICommandResult;

public record MessageResult(MessageContent Content, ButtonConfig? Buttons = null) : ICommandResult
{
    public static MessageResult CreatePrompt(MessageContent initialContent, Func<ValueTask<MessageContent>> confirm, Func<ValueTask<MessageContent>>? cancel = null)
    {
        cancel ??= () => new(new MessageContent(EmbedFactory.CreateError("👍 Operation cancelled.")));

        async ValueTask<IButtonClickResult> Confirm(string userId)
        {
            var content = await confirm();
            return new UpdateMessage(new(content));
        }

        async ValueTask<IButtonClickResult> Cancel(string userId)
        {
            var content = await cancel!();
            return new UpdateMessage(new(content));
        }

        return new MessageResult(initialContent, new([
            new ButtonResult(new("confirm", ButtonStyle.Success, Label: "Confirm"), Confirm),
            new ButtonResult(new("cancel", ButtonStyle.Danger, Label: "Cancel"), Cancel),
        ]));
    }

    public record ButtonConfig(IReadOnlyList<ButtonResult> Buttons, TimeSpan? ListenToClicksFor = null, Func<ValueTask<MessageResult>>? OnEnded = null);

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

public record CreateModalResult(string Id, string Title, IReadOnlyList<TextInput> TextInputs, Func<ModalSubmit, ValueTask<MessageResult>> SubmitAction, bool IsPrivateResponse) : ICommandResult;
