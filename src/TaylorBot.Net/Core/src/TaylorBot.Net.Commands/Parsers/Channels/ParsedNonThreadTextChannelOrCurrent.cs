using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedNonThreadTextChannelOrCurrent(ITextChannel Channel);

public class NonThreadTextChannellOrCurrentParser(TextChannelOrCurrentParser textChannelOrCurrentParser) : IOptionParser<ParsedNonThreadTextChannelOrCurrent>
{
    public async ValueTask<Result<ParsedNonThreadTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsed = await textChannelOrCurrentParser.ParseAsync(context, optionValue, resolved);

        if (parsed)
        {
            return parsed.Value.Channel is not IThreadChannel thread ?
                new ParsedNonThreadTextChannelOrCurrent(parsed.Value.Channel) :
                Error(new ParsingFailed($"Channel '{thread.Name}' is a thread! Please use another text channel."));
        }
        else
        {
            return Error(parsed.Error);
        }
    }
}
