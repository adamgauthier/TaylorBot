using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Users
{
    public record ParsedMemberNotAuthorAndBot(IGuildUser Member);

    public class MemberNotAuthorAndBotParser : IOptionParser<ParsedMemberNotAuthorAndBot>
    {
        private readonly MemberNotAuthorParser _memberNotAuthorParser;

        public MemberNotAuthorAndBotParser(MemberNotAuthorParser memberNotAuthorParser)
        {
            _memberNotAuthorParser = memberNotAuthorParser;
        }

        public async ValueTask<Result<ParsedMemberNotAuthorAndBot, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var parsedMember = await _memberNotAuthorParser.ParseAsync(context, optionValue);
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
}
