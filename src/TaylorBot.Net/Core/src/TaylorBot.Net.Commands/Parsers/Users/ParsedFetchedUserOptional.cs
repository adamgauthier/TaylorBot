using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedFetchedUserOptional(IUser? User);

public class FetchedUserOptionalParser(FetchedUserParser userParser) : IOptionParser<ParsedFetchedUserOptional>
{
    public async ValueTask<Result<ParsedFetchedUserOptional, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsed = await userParser.ParseAsync(context, optionValue, resolved);
            if (parsed)
            {
                return new ParsedFetchedUserOptional(parsed.Value.User);
            }
            else
            {
                return Error(parsed.Error);
            }
        }
        else
        {
            return new ParsedFetchedUserOptional(null);
        }
    }
}
