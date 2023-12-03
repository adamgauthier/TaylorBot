using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedTextChannelOrCurrent(ITextChannel Channel);

public class TextChannelOrCurrentParser(ITaylorBotClient taylorBotClient) : IOptionParser<ParsedTextChannelOrCurrent>
{
    public async ValueTask<Result<ParsedTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var channel = await taylorBotClient.ResolveRequiredChannelAsync(new(optionValue.Value.GetString()!));
            if (channel is not ITextChannel text)
            {
                return Error(new ParsingFailed($"Channel '{channel.Name}' is not a text channel."));
            }
            return new ParsedTextChannelOrCurrent(text);
        }
        else
        {
            var channel = await taylorBotClient.ResolveRequiredChannelAsync(new(context.Channel.Id));
            if (channel is not ITextChannel text)
            {
                return Error(new ParsingFailed($"The current channel {channel.Name} is not part of a server."));
            }
            return new ParsedTextChannelOrCurrent(text);
        }
    }
}
