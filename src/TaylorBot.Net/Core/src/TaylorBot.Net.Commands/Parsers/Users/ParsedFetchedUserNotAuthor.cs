using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedFetchedUserNotAuthor(IUser User);

public class FetchedUserNotAuthorParser(FetchedUserParser userParser) : IOptionParser<ParsedFetchedUserNotAuthor>
{
    public async ValueTask<Result<ParsedFetchedUserNotAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await userParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return parsedUser.Value.User.Id != context.User.Id ?
                new ParsedFetchedUserNotAuthor(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be yourself."));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
