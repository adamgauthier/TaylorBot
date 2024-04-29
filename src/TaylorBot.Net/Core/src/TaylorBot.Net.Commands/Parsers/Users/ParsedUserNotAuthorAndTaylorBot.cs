using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUserNotAuthorAndTaylorBot(DiscordUser User);

public class UserNotAuthorAndTaylorBotParser(UserNotAuthorParser userNotAuthorParser) : IOptionParser<ParsedUserNotAuthorAndTaylorBot>
{
    public async ValueTask<Result<ParsedUserNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await userNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return parsedUser.Value.User.Id != context.BotUser.Id ?
                new ParsedUserNotAuthorAndTaylorBot(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be TaylorBot 🤭"));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
