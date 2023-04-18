using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberNotAuthorAndTaylorBot(IGuildUser Member);

public class MemberNotAuthorAndTaylorBotParser : IOptionParser<ParsedMemberNotAuthorAndTaylorBot>
{
    private readonly MemberNotAuthorParser _memberNotAuthorParser;

    public MemberNotAuthorAndTaylorBotParser(MemberNotAuthorParser memberNotAuthorParser)
    {
        _memberNotAuthorParser = memberNotAuthorParser;
    }

    public async ValueTask<Result<ParsedMemberNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedMember = await _memberNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedMember)
        {
            return parsedMember.Value.Member.Id != context.BotUser.Id ?
                new ParsedMemberNotAuthorAndTaylorBot(parsedMember.Value.Member) :
                Error(new ParsingFailed("Member can't be TaylorBot."));
        }
        else
        {
            return Error(parsedMember.Error);
        }
    }
}
