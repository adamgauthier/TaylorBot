using Discord;
using OperationResult;
using System.Text.Json;
using TaylorBot.Net.Core.Channel;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Parsers.Channels;

public record ParsedChannelOrCurrent(DiscordChannel Channel);

public class ChannelOrCurrentParser(ISpamChannelRepository spamChannelRepository, InteractionMapper interactionMapper) : IOptionParser<ParsedChannelOrCurrent>
{
    public async ValueTask<Result<ParsedChannelOrCurrent, ParsingFailed>> ParseAsync(RunContext context, JsonElement? optionValue, Interaction.Resolved? resolved)
    {
        var option = optionValue?.GetString();
        if (option != null)
        {
            if (resolved?.channels?.TryGetValue(option, out var resolvedChannel) == true)
            {
                await TrackTextChannelAsync(context, resolvedChannel);

                return new ParsedChannelOrCurrent(interactionMapper.ToChannel(resolvedChannel));
            }
            else
            {
                throw new InvalidOperationException($"Can't find {option} in resolved data {JsonSerializer.Serialize(resolved)}");
            }
        }
        else
        {
            return new ParsedChannelOrCurrent(context.Channel);
        }
    }

    public async Task<GuildTextChannel?> TrackTextChannelAsync(RunContext context, Interaction.PartialChannel resolvedChannel)
    {
        var guildId = resolvedChannel.guild_id ?? context.Guild?.Id?.ToString();
        if (guildId == null ||
            !IsGuildTextChannel(resolvedChannel))
        {
            return null;
        }

        GuildTextChannel channel = new(resolvedChannel.id, guildId, (ChannelType)resolvedChannel.type);

        // Current channel is already tracked in preconditions
        if (resolvedChannel.id != context.Channel.Id &&
            context.Guild?.Fetched != null)
        {
            _ = await spamChannelRepository.InsertOrGetIsSpamChannelAsync(channel);
        }

        return channel;
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
