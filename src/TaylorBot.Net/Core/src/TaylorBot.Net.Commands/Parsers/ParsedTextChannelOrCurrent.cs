using Discord;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.Commands.Parsers
{
    public record ParsedTextChannelOrCurrent(ITextChannel Channel) : IParseResult;

    public class TextChannelOrCurrentParser : IOptionParser<ParsedTextChannelOrCurrent>
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public TextChannelOrCurrentParser(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public async ValueTask<IParseResult> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (optionValue.HasValue)
            {
                var channel = await _taylorBotClient.ResolveRequiredChannelAsync(new(optionValue.Value.GetString()!));
                if (channel is not ITextChannel text)
                {
                    return new ParsingFailed($"Channel '{channel.Name}' is not a text channel.");
                }
                return new ParsedTextChannelOrCurrent(text);
            }
            else
            {
                var channel = context.Channel;
                if (channel is not ITextChannel text)
                {
                    return new ParsingFailed($"The current channel {channel.Name} is not part of a server.");
                }
                return new ParsedTextChannelOrCurrent(text);
            }
        }
    }
}
