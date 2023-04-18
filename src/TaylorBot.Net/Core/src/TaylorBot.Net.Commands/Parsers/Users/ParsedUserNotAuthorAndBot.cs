using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUserNotAuthorAndBot(IUser User);

public class UserNotAuthorAndBotParser : IOptionParser<ParsedUserNotAuthorAndBot>
{
    private readonly UserNotAuthorParser _userNotAuthorParser;

    public UserNotAuthorAndBotParser(UserNotAuthorParser userNotAuthorParser)
    {
        _userNotAuthorParser = userNotAuthorParser;
    }

    public async ValueTask<Result<ParsedUserNotAuthorAndBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await _userNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return !parsedUser.Value.User.IsBot ?
                new ParsedUserNotAuthorAndBot(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be a bot."));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
