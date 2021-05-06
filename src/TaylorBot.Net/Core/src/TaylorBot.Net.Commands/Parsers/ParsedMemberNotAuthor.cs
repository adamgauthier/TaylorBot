using Discord;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedMemberNotAuthor(IGuildUser Member) : IParseResult;

    public class MemberNotAuthorParser : IOptionParser<ParsedMemberNotAuthor>
    {
        private readonly MemberParser _memberParser;

        public MemberNotAuthorParser(MemberParser memberParser)
        {
            _memberParser = memberParser;
        }

        public async ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            var parsedMember = (ParsedMember)await _memberParser.ParseAsync(context, optionValue);

            return parsedMember.Member.Id != context.User.Id ?
                new ParsedMemberNotAuthor(parsedMember.Member) :
                new ParsingFailed("Member can't be yourself.");
        }
    }
}
