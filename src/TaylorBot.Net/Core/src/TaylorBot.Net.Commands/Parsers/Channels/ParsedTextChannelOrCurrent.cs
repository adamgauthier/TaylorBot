using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using static OperationResult.Helpers;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedTextChannelOrCurrent(GuildTextChannel Channel);

public class TextChannelOrCurrentParser(ISpamChannelRepository spamChannelRepository) : IOptionParser<ParsedTextChannelOrCurrent>
{
    public async ValueTask<Result<ParsedTextChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option != null)
        {
            if (resolved?.channels?.TryGetValue(option, out var resolvedChannel) == true)
            {
                var guildId = resolvedChannel.guild_id ?? context.Guild?.Id?.ToString();

                if (IsGuildTextChannel(resolvedChannel) && guildId != null)
                {
                    GuildTextChannel channel = new(resolvedChannel.id, guildId, (ChannelType)resolvedChannel.type);

                    // Command channel is already tracked
                    if (channel.Id != context.Channel.Id && context.Guild?.Fetched != null)
                    {
                        _ = await spamChannelRepository.InsertOrGetIsSpamChannelAsync(channel);
                    }

                    return new ParsedTextChannelOrCurrent(channel);
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

    private static bool IsGuildTextChannel(Interaction.PartialChannel resolvedChannel)
    {
        var channelType = (ChannelType)resolvedChannel.type;

        return channelType is
            ChannelType.Text or
            ChannelType.Voice or
            ChannelType.News or
            ChannelType.NewsThread or
            ChannelType.PublicThread or
            ChannelType.PrivateThread or
            ChannelType.Stage;
    }
}
