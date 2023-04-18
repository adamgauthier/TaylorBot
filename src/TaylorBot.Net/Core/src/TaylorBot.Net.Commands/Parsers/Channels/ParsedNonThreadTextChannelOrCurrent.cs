using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedNonThreadTextChannelOrCurrent(ITextChannel Channel);

public class NonThreadTextChannellOrCurrentParser : IOptionParser<ParsedNonThreadTextChannelOrCurrent>
{
    private readonly TextChannelOrCurrentParser _textChannelOrCurrentParser;

    public NonThreadTextChannellOrCurrentParser(TextChannelOrCurrentParser textChannelOrCurrentParser)
    {
        _textChannelOrCurrentParser = textChannelOrCurrentParser;
    }

    public async ValueTask<Result<ParsedNonThreadTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsed = await _textChannelOrCurrentParser.ParseAsync(context, optionValue, resolved);

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
