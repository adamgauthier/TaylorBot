using Discord;
using TaylorBot.Net.Commands.DiscordNet.PageMessages;
using TaylorBot.Net.Commands.PostExecution;

namespace TaylorBot.Net.Commands;

public interface ICommandResult { }


public record EmbedResult(Embed Embed) : ICommandResult;

public record EmptyResult() : ICommandResult;

public record RateLimitedResult(string FriendlyLimitName, long Uses, uint Limit) : ICommandResult;

public record PageMessageResult(PageMessage PageMessage) : ICommandResult;


public record MessageResult(MessageResponse Message) : ICommandResult
{
    public static MessageResult CreatePrompt(MessageContent content, InteractionCustomId confirmButtonId)
    {
        return new(MessageResponse.CreatePrompt(content, confirmButtonId));
    }
}

public enum TextInputStyle { Short, Paragraph }

public record TextInput(string Id, TextInputStyle Style, string Label, bool Required = true, int? MinLength = null, int? MaxLength = null);

public record CreateModalResult(string Id, string Title, IReadOnlyList<TextInput> TextInputs) : ICommandResult;
