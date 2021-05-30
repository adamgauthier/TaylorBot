using Discord;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.PageMessages;

namespace TaylorBot.Net.Commands
{
    public interface ICommandResult { }


    public record EmbedResult(Embed Embed) : ICommandResult;

    public record EmptyResult() : ICommandResult;

    public record RateLimitedResult(string FriendlyLimitName, long Uses, uint Limit) : ICommandResult;

    public record PageMessageResult(PageMessage PageMessage) : ICommandResult;

    public record PromptEmbedResult(Embed Prompt, Func<ValueTask<Embed>> Confirm, Func<ValueTask<Embed>>? Cancel = null) : ICommandResult;
}
