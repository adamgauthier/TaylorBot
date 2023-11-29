using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberNotAuthor(IGuildUser Member);

public class MemberNotAuthorParser : IOptionParser<ParsedMemberNotAuthor>
{
    private readonly MemberParser _memberParser;

    public MemberNotAuthorParser(MemberParser memberParser)
    {
        _memberParser = memberParser;
    }

    public async ValueTask<Result<ParsedMemberNotAuthor, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedMember = await _memberParser.ParseAsync(context, optionValue, resolved);
        if (parsedMember)
        {
            return parsedMember.Value.Member.Id != context.User.Id ?
                new ParsedMemberNotAuthor(parsedMember.Value.Member) :
                Error(new ParsingFailed("Member can't be yourself."));
        }
        else
        {
            return Error(parsedMember.Error);
        }
    }
}
