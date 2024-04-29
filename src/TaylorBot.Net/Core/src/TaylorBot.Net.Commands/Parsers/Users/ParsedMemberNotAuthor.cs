using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberNotAuthor(DiscordMember Member);

public class MemberNotAuthorParser(MemberParser memberParser) : IOptionParser<ParsedMemberNotAuthor>
{
    public async ValueTask<Result<ParsedMemberNotAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedMember = await memberParser.ParseAsync(context, optionValue, resolved);
        if (parsedMember)
        {
            return parsedMember.Value.Member.User.Id != context.User.Id ?
                new ParsedMemberNotAuthor(parsedMember.Value.Member) :
                Error(new ParsingFailed("Member can't be yourself 🤭"));
        }
        else
        {
            return Error(parsedMember.Error);
        }
    }
}
