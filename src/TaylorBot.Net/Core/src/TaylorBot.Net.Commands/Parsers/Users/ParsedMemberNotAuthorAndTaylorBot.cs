using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.User;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users;

public record ParsedMemberNotAuthorAndTaylorBot(DiscordMember Member);

public class MemberNotAuthorAndTaylorBotParser(MemberNotAuthorParser memberNotAuthorParser, Lazy<ITaylorBotClient> taylorBotClient) : IOptionParser<ParsedMemberNotAuthorAndTaylorBot>
{
    public async ValueTask<Result<ParsedMemberNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsedMember = await memberNotAuthorParser.ParseAsync(context, optionValue, resolved);
        if (parsedMember)
        {
            return parsedMember.Value.Member.User.Id != taylorBotClient.Value.DiscordShardedClient.CurrentUser.Id ?
                new ParsedMemberNotAuthorAndTaylorBot(parsedMember.Value.Member) :
                Error(new ParsingFailed("Member can't be TaylorBot 🤭"));
        }
        else
        {
            return Error(parsedMember.Error);
        }
    }
}
