using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users
{
    public record ParsedUserNotAuthor(IUser User);

    public class UserNotAuthorParser : IOptionParser<ParsedUserNotAuthor>
    {
        private readonly UserParser _userParser;

        public UserNotAuthorParser(UserParser userParser)
        {
            _userParser = userParser;
        }

        public async ValueTask<Result<ParsedUserNotAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var parsedUser = await _userParser.ParseAsync(context, optionValue);
            if (parsedUser)
            {
                return parsedUser.Value.User.Id != context.User.Id ?
                    new ParsedUserNotAuthor(parsedUser.Value.User) :
                    Error(new ParsingFailed("User can't be yourself."));
            }
            else
            {
                return Error(parsedUser.Error);
            }
        }
    }
}
