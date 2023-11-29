using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUserNotAuthorAndTaylorBot(IUser User);

public class UserNotAuthorAndTaylorBotParser : IOptionParser<ParsedUserNotAuthorAndTaylorBot>
{
    private readonly UserNotAuthorParser _userNotAuthorParser;

    public UserNotAuthorAndTaylorBotParser(UserNotAuthorParser userNotAuthorParser)
    {
        _userNotAuthorParser = userNotAuthorParser;
    }

    public async ValueTask<Result<ParsedUserNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await _userNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return parsedUser.Value.User.Id != context.BotUser.Id ?
                new ParsedUserNotAuthorAndTaylorBot(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be TaylorBot."));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
