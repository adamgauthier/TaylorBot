﻿using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUserNotAuthorAndBot(DiscordUser User);

public class UserNotAuthorAndBotParser(UserNotAuthorParser userNotAuthorParser) : IOptionParser<ParsedUserNotAuthorAndBot>
{
    public async ValueTask<Result<ParsedUserNotAuthorAndBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedUser = await userNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            return !parsedUser.Value.User.IsBot ?
                new ParsedUserNotAuthorAndBot(parsedUser.Value.User) :
                Error(new ParsingFailed("User can't be a bot 🤭"));
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
