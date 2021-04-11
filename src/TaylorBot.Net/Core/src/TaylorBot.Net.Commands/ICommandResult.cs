using Discord;
using TaylorBot.Net.Commands.PageMessages;

namespace TaylorBot.Net.Commands
{
    public interface ICommandResult { }


    public record EmbedResult(Embed Embed) : ICommandResult;

    public record EmptyResult() : ICommandResult;

    public record RateLimitedResult(string FriendlyLimitName, long Uses, uint Limit) : ICommandResult;

    public record PageMessageResult(PageMessage PageMessage) : ICommandResult;
}
