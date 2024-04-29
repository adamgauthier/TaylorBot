using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedUserOrAuthor(DiscordUser User);

public class UserOrAuthorParser(UserParser userParser) : IOptionParser<ParsedUserOrAuthor>
{
    public async ValueTask<Result<ParsedUserOrAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsedUser = await userParser.ParseAsync(context, optionValue, resolved);
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
