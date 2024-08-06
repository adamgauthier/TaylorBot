using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedTextChannelOrCurrent(GuildTextChannel Channel);

public class TextChannelOrCurrentParser(ChannelOrCurrentParser channelOrCurrentParser) : IOptionParser<ParsedTextChannelOrCurrent>
{
    public async ValueTask<Result<ParsedTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option != null)
        {
            if (resolved?.channels?.TryGetValue(option, out var resolvedChannel) == true)
            {
                var guildTextChannel = await channelOrCurrentParser.TrackTextChannelAsync(context, resolvedChannel);
                if (guildTextChannel != null)
                {
                    return new ParsedTextChannelOrCurrent(guildTextChannel);
                }
                else
                {
                    return Error(new ParsingFailed($"Channel {MentionUtils.MentionChannel(new SnowflakeId(resolvedChannel.id))} is not a text channel."));
                }
            }
            else
            {
                throw new InvalidOperationException($"Can't find {option} in resolved data {JsonSerializer.Serialize(resolved)}");
            }
        }
        else
        {
            if (context.Guild == null)
            {
                return Error(new ParsingFailed($"The current channel {MentionUtils.MentionChannel(context.Channel.Id)} is not part of a server."));
            }
            return new ParsedTextChannelOrCurrent(new(context.Channel.Id, context.Guild.Id, context.Channel.Type));
        }
    }
}
