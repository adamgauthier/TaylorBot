using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedUser(IUser User);

    public class UserParser : IOptionParser<ParsedUser>
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public UserParser(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public async ValueTask<Result<ParsedUser, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return Error(new ParsingFailed("User option is required."));
            }

            var user = await _taylorBotClient.ResolveRequiredUserAsync(new(optionValue.Value.GetString()!));
            return new ParsedUser(user);
        }
    }
}
