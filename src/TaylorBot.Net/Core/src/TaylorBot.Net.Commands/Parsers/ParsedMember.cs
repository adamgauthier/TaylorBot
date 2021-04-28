using Discord;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedMember(IGuildUser Member) : IParseResult;

    public class MemberParser : IOptionParser<ParsedMember>
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public MemberParser(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public async ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (!optionValue.HasValue)
            {
                return new ParsingFailed("Member option is required.");
            }
            if (context.Guild == null)
            {
                return new ParsingFailed("Member option can only be used in a server.");
            }

            var member = await _taylorBotClient.ResolveGuildUserAsync(context.Guild, new(optionValue.Value.GetString()!));

            if (member == null)
            {
                return new ParsingFailed($"Did not find member in the current server ({context.Guild.Name}).");
            }

            return new ParsedMember(member);
        }
    }
}
