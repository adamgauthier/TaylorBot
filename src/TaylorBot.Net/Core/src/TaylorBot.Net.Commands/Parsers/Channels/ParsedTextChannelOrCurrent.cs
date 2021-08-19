using Discord;
using OperationResult;
using System.Text.Json;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels
{
    public record ParsedTextChannelOrCurrent(ITextChannel Channel);

    public class TextChannelOrCurrentParser : IOptionParser<ParsedTextChannelOrCurrent>
    {
        private readonly ITaylorBotClient _taylorBotClient;

        public TextChannelOrCurrentParser(ITaylorBotClient taylorBotClient)
        {
            _taylorBotClient = taylorBotClient;
        }

        public async ValueTask<Result<ParsedTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue)
        {
            if (optionValue.HasValue)
            {
                var channel = await _taylorBotClient.ResolveRequiredChannelAsync(new(optionValue.Value.GetString()!));
                if (channel is not ITextChannel text)
                {
                    return Error(new ParsingFailed($"Channel '{channel.Name}' is not a text channel."));
                }
                return new ParsedTextChannelOrCurrent(text);
            }
            else
            {
                var channel = await _taylorBotClient.ResolveRequiredChannelAsync(new(context.Channel.Id));
                if (channel is not ITextChannel text)
                {
                    return Error(new ParsingFailed($"The current channel {channel.Name} is not part of a server."));
                }
                return new ParsedTextChannelOrCurrent(text);
            }
        }
    }
}
