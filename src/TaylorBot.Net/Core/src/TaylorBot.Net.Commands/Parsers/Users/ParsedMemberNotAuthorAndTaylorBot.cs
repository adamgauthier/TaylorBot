using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberNotAuthorAndTaylorBot(IGuildUser Member);

public class MemberNotAuthorAndTaylorBotParser(MemberNotAuthorParser memberNotAuthorParser) : IOptionParser<ParsedMemberNotAuthorAndTaylorBot>
{
    public async ValueTask<Result<ParsedMemberNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedMember = await memberNotAuthorParser.ParseAsync(context, optionValue, resolved);
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
