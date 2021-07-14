using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users
{
    public record ParsedOptionalUser(IUser? User);

    public class OptionalUserParser : IOptionParser<ParsedOptionalUser>
    {
        private readonly UserParser _userParser;

        public OptionalUserParser(UserParser userParser)
        {
            _userParser = userParser;
        }

        public async ValueTask<Result<ParsedOptionalUser, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (optionValue.HasValue)
            {
                var parsed = await _userParser.ParseAsync(context, optionValue);
                if (parsed)
                {
                    return new ParsedOptionalUser(parsed.Value.User);
                }
                else
                {
                    return Error(parsed.Error);
                }
            }
            else
            {
                return new ParsedOptionalUser(null);
            }
        }
    }
}
