﻿using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users
{
    public record ParsedUserOrAuthor(IUser User);

    public class UserOrAuthorParser : IOptionParser<ParsedUserOrAuthor>
    {
        private readonly UserParser _userParser;

        public UserOrAuthorParser(UserParser userParser)
        {
            _userParser = userParser;
        }

        public async ValueTask<Result<ParsedUserOrAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (optionValue.HasValue)
            {
                var parsedUser = await _userParser.ParseAsync(context, optionValue);
                if (parsedUser)
                {
                    return new ParsedUserOrAuthor(parsedUser.Value.User);
                }
                else
                {
                    return Error(parsedUser.Error);
                }
            }
            else
            {
                return new ParsedUserOrAuthor(context.User);
            }
        }
    }
}