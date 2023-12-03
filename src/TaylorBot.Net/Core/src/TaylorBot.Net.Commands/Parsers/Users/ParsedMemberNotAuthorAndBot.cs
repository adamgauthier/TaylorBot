using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberNotAuthorAndBot(IGuildUser Member);

public class MemberNotAuthorAndBotParser(MemberNotAuthorParser memberNotAuthorParser) : IOptionParser<ParsedMemberNotAuthorAndBot>
{
    public async ValueTask<Result<ParsedMemberNotAuthorAndBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedMember = await memberNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedMember)
        {
            return !parsedMember.Value.Member.IsBot ?
                new ParsedMemberNotAuthorAndBot(parsedMember.Value.Member) :
                Error(new ParsingFailed("Member can't be a bot."));
        }
        else
        {
            return Error(parsedMember.Error);
        }
    }
}
