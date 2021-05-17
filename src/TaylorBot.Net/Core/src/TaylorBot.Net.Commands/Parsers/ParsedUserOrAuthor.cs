using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedUserOrAuthor(IUser User);

    public class UserOrAuthorParser : IOptionParser<ParsedUserOrAuthor>
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public UserOrAuthorParser(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public async ValueTask<Result<ParsedUserOrAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (optionValue.HasValue)
            {
                var user = await _taylorBotClient.ResolveRequiredUserAsync(new(optionValue.Value.GetString()!));
                return new ParsedUserOrAuthor(user);
            }
            else
            {
                return new ParsedUserOrAuthor(context.User);
            }
        }
    }
}
