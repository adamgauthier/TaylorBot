using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMember(DiscordMember Member);

public class MemberParser(UserParser userParser) : IOptionParser<ParsedMember>
{
    public async ValueTask<Result<ParsedMember, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option == null)
        {
            return Error(new ParsingFailed("Member option is required 🤔"));
        }

        if (context.Guild == null)
        {
            return Error(new ParsingFailed("This command can only be used in a server 🤭"));
        }

        var parsedUser = await userParser.ParseAsync(context, optionValue, resolved);
        if (parsedUser)
        {
            if (parsedUser.Value.User.TryGetMember(out var member))
            {
                return new ParsedMember(member);
            }
            else
            {
                return Error(new ParsingFailed("User must be in the current server 🤭"));
            }
        }
        else
        {
            return Error(parsedUser.Error);
        }
    }
}
