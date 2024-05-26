using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedNonThreadTextChannelOrCurrent(GuildTextChannel Channel);

public class NonThreadTextChannellOrCurrentParser(TextChannelOrCurrentParser textChannelOrCurrentParser) : IOptionParser<ParsedNonThreadTextChannelOrCurrent>
{
    public async ValueTask<Result<ParsedNonThreadTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var parsed = await textChannelOrCurrentParser.ParseAsync(context, optionValue, resolved);

        if (parsed)
        {
            return parsed.Value.Channel.Type is ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.NewsThread
                ? Error(new ParsingFailed($"Channel {MentionUtils.MentionChannel(parsed.Value.Channel.Id)} is a thread! Please use another text channel."))
                : new ParsedNonThreadTextChannelOrCurrent(parsed.Value.Channel);
        }
        else
        {
            return Error(parsed.Error);
        }
    }
}
