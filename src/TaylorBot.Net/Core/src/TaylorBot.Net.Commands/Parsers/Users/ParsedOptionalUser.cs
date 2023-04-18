using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedOptionalUser(IUser? User);

public class OptionalUserParser : IOptionParser<ParsedOptionalUser>
{
    private readonly UserParser _userParser;

    public OptionalUserParser(UserParser userParser)
    {
        _userParser = userParser;
    }

    public async ValueTask<Result<ParsedOptionalUser, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsed = await _userParser.ParseAsync(context, optionValue, resolved);
            if (parsed)
            {
                return new ParsedOptionalUser(parsed.Value.User);
            }
            else
            {
                return Error(parsed.Error);
            }
        }
        else
        {
            return new ParsedOptionalUser(null);
        }
    }
}
