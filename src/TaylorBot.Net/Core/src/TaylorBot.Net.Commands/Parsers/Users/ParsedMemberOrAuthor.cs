using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberOrAuthor(DiscordMember Member);

public class MemberOrAuthorParser(MemberParser memberParser) : IOptionParser<ParsedMemberOrAuthor>
{
    public async ValueTask<Result<ParsedMemberOrAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        if (optionValue.HasValue)
        {
            var parsedMember = await memberParser.ParseAsync(context, optionValue, resolved);
            if (parsedMember)
            {
                return new ParsedMemberOrAuthor(parsedMember.Value.Member);
            }
            else
            {
                return Error(parsedMember.Error);
            }
        }
        else
        {
            if (context.User.TryGetMember(out var member))
            {
                return new ParsedMemberOrAuthor(member);
            }
            else
            {
                return Error(new ParsingFailed("This command can only be used in a server 🤭"));
            }
        }
    }
}
