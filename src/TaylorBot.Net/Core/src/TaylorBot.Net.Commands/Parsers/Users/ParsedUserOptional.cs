using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUserOptional(DiscordUser? User);

public class UserOptionalParser(UserParser userParser) : IOptionParser<ParsedUserOptional>
{
    public async ValueTask<Result<ParsedUserOptional, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsed = await userParser.ParseAsync(context, optionValue, resolved);
            if (parsed)
            {
                return new ParsedUserOptional(parsed.Value.User);
            }
            else
            {
                return Error(parsed.Error);
            }
        }
        else
        {
            return new ParsedUserOptional(null);
        }
    }
}
