using Discord;
using System.Globalization;
using System.Text;
using System.Text.Json;
using TaylorBot.Net.Commands.Discord.Program.Services;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class InspectChannelSlashCommand(Lazy<ITaylorBotClient> taylorBot, ChannelTypeStringMapper channelTypeStringMapper) : ISlashCommand<InspectChannelSlashCommand.Options>
{
    public static string CommandName => "inspect channel";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var c = options.channel.Channel;
                var createdAt = SnowflakeUtils.FromSnowflake(c.Id);

                var metadata = options.channel.Metadata;
                var channel = metadata?.name != null
                    ? await ((IDiscordClient)taylorBot.Value.DiscordShardedClient).GetChannelAsync(c.Id, CacheMode.CacheOnly)
                    : await taylorBot.Value.ResolveRequiredChannelAsync(c.Id);
                var name = metadata?.name ?? channel?.Name
                    ?? throw new InvalidOperationException($"No name available for channel '{c.Id}'.");
                var nsfw = metadata?.nsfw ?? (channel is ITextChannel textChannel && textChannel.IsNsfw);
                var guild = (channel as IGuildChannel)?.Guild ?? context.Guild?.Fetched;

                var embed = new EmbedBuilder()
                    .WithAuthor(nsfw ? $"{name} 🔞" : name)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .AddField("Id", $"`{c.Id}`", inline: true)
                    .AddField("Type", channelTypeStringMapper.MapChannelToTypeString(c.Type), inline: true)
                    .AddField("Created", createdAt.FormatDetailedWithRelative());

                var parentId = metadata != null && metadata.parent_id.ValueKind != JsonValueKind.Undefined
                    ? metadata.parent_id.ValueKind == JsonValueKind.Null
                        ? null
                        : ulong.Parse(metadata.parent_id.GetString()!, CultureInfo.InvariantCulture)
                    : (channel as INestedChannel)?.CategoryId;
                if (parentId.HasValue)
                {
                    var parent = guild != null ? await guild.GetChannelAsync(parentId.Value, CacheMode.CacheOnly) : null;
                    const ChannelFlags obfuscated = (ChannelFlags)(1 << 17);
                    var category = parent != null && (parent.Flags & obfuscated) == 0
                        ? $"{parent.Name} (`{parentId.Value}`)"
                        : $"`{parentId.Value}`";
                    embed.AddField("Category", category, inline: true);
                }

                if (guild != null)
                {
                    embed.AddField("Server", $"{guild.Name} (`{guild.Id}`)", inline: true);
                    switch (c.Type)
                    {
                        case ChannelType.Voice or ChannelType.Stage when channel is IVoiceChannel voice:
                            embed
                                .AddField("Bitrate", $"{voice.Bitrate} bps", inline: true)
                                .AddField("User Limit", voice.UserLimit.HasValue ? $"{voice.UserLimit.Value}" : "None", inline: true);
                            break;

                        case ChannelType.Text or ChannelType.News or ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread:
                            var topic = metadata != null && metadata.topic.ValueKind != JsonValueKind.Undefined
                                ? metadata.topic.GetString()
                                : (channel as ITextChannel)?.Topic;
                            embed.AddField("Topic", string.IsNullOrEmpty(topic) ? "None" : topic);
                            break;

                        case ChannelType.Category:
                            var channels = await guild.GetChannelsAsync(CacheMode.CacheOnly);
                            var children = channels.OfType<INestedChannel>()
                                .Where(child => child.CategoryId == c.Id)
                                .Select(c => $"https://discord.com/channels/{c.GuildId}/{c.Id}")
                                .ToList();
                            embed.AddField("Channels", FormatChannelLinks(children));
                            break;

                        default:
                            break;
                    }
                }

                return new EmbedResult(embed.Build());
            }
        ));
    }

    private static string FormatChannelLinks(IReadOnlyList<string> links)
    {
        if (links.Count == 0)
        {
            return "None";
        }

        var complete = string.Join(' ', links);
        if (complete.Length <= EmbedFieldBuilder.MaxFieldValueLength)
        {
            return complete;
        }

        const string suffix = " ...";
        StringBuilder result = new();
        foreach (var link in links)
        {
            var separatorLength = result.Length > 0 ? 1 : 0;
            if (result.Length + separatorLength + link.Length + suffix.Length > EmbedFieldBuilder.MaxFieldValueLength)
            {
                break;
            }
            if (result.Length > 0)
            {
                result.Append(' ');
            }
            result.Append(link);
        }
        return result.Append(suffix).ToString();
    }
}
