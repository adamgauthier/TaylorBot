using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedFetchedUserNotAuthorAndBot(IUser User);

public class FetchedUserNotAuthorAndBotParser(FetchedUserNotAuthorParser userNotAuthorParser) : IOptionParser<ParsedFetchedUserNotAuthorAndBot>
{
    public async ValueTask<Result<ParsedFetchedUserNotAuthorAndBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await userNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return !parsedUser.Value.User.IsBot ?
                new ParsedFetchedUserNotAuthorAndBot(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be a bot."));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
