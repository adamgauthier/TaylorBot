using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands
{
    public interface ICommandResult { }


    public record EmbedResult(Embed Embed) : ICommandResult;

    public record EmptyResult() : ICommandResult;

    public record RateLimitedResult(string FriendlyLimitName, long Uses, uint Limit) : ICommandResult;

    public record PageMessageResult(PageMessage PageMessage) : ICommandResult;

    public record MessageResult(MessageContent Content, IReadOnlyList<MessageResult.ButtonResult>? Buttons = null) : ICommandResult
    {
        public static MessageResult CreatePrompt(MessageContent initialContent, Func<ValueTask<MessageContent>> confirm, Func<ValueTask<MessageContent>>? cancel = null)
        {
            cancel ??= () => new(new MessageContent(EmbedFactory.CreateError("👍 Operation cancelled.")));

            async ValueTask<MessageResult?> Confirm()
            {
                var content = await confirm();
                return new(content);
            }

            async ValueTask<MessageResult?> Cancel()
            {
                var content = await cancel();
                return new(content);
            }

            return new MessageResult(initialContent, new[] {
                new ButtonResult(new("confirm", ButtonStyle.Success, Label: "Confirm"), Confirm),
                new ButtonResult(new("cancel", ButtonStyle.Danger, Label: "Cancel"), Cancel),
            });
        }

        public record ButtonResult(Button Button, Func<ValueTask<MessageResult?>> Action);
    }

    public enum TextInputStyle { Short, Paragraph }

    public record TextInput(string Id, TextInputStyle Style, string Label);

    public record CreateModalResult(string Id, string Title, IReadOnlyList<TextInput> TextInputs, Func<ModalSubmit, ValueTask<MessageResult>> SubmitAction, bool IsPrivateResponse) : ICommandResult;
}
