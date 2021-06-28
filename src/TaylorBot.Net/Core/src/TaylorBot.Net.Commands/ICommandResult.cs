using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PageMessages;
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
            if (cancel == null)
                cancel = () => new(new MessageContent(EmbedFactory.CreateError("👍 Operation cancelled.")));

            return new MessageResult(initialContent, new[] {
                new ButtonResult(new("confirm", ButtonStyle.Success, Label: "Confirm"), confirm),
                new ButtonResult(new("cancel", ButtonStyle.Danger, Label: "Cancel"), cancel),
            });
        }

        public record ButtonResult(Button Button, Func<ValueTask<MessageContent>> Action);
    }
}
