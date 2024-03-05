using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberOrAuthor(IGuildUser Member);

public class MemberOrAuthorParser(MemberParser memberParser) : IOptionParser<ParsedMemberOrAuthor>
{
    public async ValueTask<Result<ParsedMemberOrAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsedUser = await memberParser.ParseAsync(context, optionValue, resolved);
            if (parsedUser)
            {
                return new ParsedMemberOrAuthor(parsedUser.Value.Member);
            }
            else
            {
                return Error(parsedUser.Error);
            }
        }
        else
        {
            if (context.User is not IGuildUser member)
            {
                return Error(new ParsingFailed("Member option can only be used in a server."));
            }

            return new ParsedMemberOrAuthor(member);
        }
    }
}
