using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users
{
    public record ParsedMemberNotAuthorAndTaylorBot(IGuildUser Member);

    public class MemberNotAuthorAndTaylorBotParser : IOptionParser<ParsedMemberNotAuthorAndTaylorBot>
    {
        private readonly MemberNotAuthorParser _memberNotAuthorParser;

        public MemberNotAuthorAndTaylorBotParser(MemberNotAuthorParser memberNotAuthorParser)
        {
            _memberNotAuthorParser = memberNotAuthorParser;
        }

        public async ValueTask<Result<ParsedMemberNotAuthorAndTaylorBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var parsedMember = await _memberNotAuthorParser.ParseAsync(context, optionValue);
            if (parsedMember)
            {
                return parsedMember.Value.Member.Id != context.Client.CurrentUser.Id ?
                    new ParsedMemberNotAuthorAndTaylorBot(parsedMember.Value.Member) :
                    Error(new ParsingFailed("Member can't be TaylorBot."));
            }
            else
            {
                return Error(parsedMember.Error);
            }
        }
    }
}
