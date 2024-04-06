using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedFetchedUserNotAuthorAndTaylorBot(IUser User);

public class FetchedUserNotAuthorAndTaylorBotParser(FetchedUserNotAuthorParser userNotAuthorParser) : IOptionParser<ParsedFetchedUserNotAuthorAndTaylorBot>
{
    public async ValueTask<Result<ParsedFetchedUserNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await userNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return parsedUser.Value.User.Id != context.BotUser.Id ?
                new ParsedFetchedUserNotAuthorAndTaylorBot(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be TaylorBot."));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
