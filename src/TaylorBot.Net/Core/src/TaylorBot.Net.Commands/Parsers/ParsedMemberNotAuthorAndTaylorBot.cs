using Discord;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedMemberNotAuthorAndTaylorBot(IGuildUser Member) : IParseResult;

    public class ParsedMemberNotAuthorAndTaylorBotParser : IOptionParser<ParsedMemberNotAuthorAndTaylorBot>
    {
        private readonly MemberNotAuthorParser _memberNotAuthorParser;

        public ParsedMemberNotAuthorAndTaylorBotParser(MemberNotAuthorParser memberNotAuthorParser)
        {
            _memberNotAuthorParser = memberNotAuthorParser;
        }

        public async ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var parsedMember = (ParsedMemberNotAuthor)await _memberNotAuthorParser.ParseAsync(context, optionValue);

            return parsedMember.Member.Id != context.Client.CurrentUser.Id ?
                new ParsedMemberNotAuthorAndTaylorBot(parsedMember.Member) :
                new ParsingFailed("Member can't be TaylorBot.");
        }
    }
}
