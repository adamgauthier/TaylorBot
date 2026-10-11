using Discord;
using System.Text;
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

                var channel = await taylorBot.Value.ResolveRequiredChannelAsync(c.Id);

                var embed = new EmbedBuilder()
                    .WithAuthor(c is ITextChannel t && t.IsNsfw ? $"{channel.Name} 🔞" : channel.Name)
                    .WithColor(TaylorBotColors.SuccessColor)
                    .AddField("Id", $"`{c.Id}`", inline: true)
                    .AddField("Type", channelTypeStringMapper.MapChannelToTypeString(channel), inline: true)
                    .AddField("Created", createdAt.FormatDetailedWithRelative());

                if (channel is INestedChannel nested && nested.CategoryId.HasValue)
                {
                    var parent = await nested.Guild.GetChannelAsync(nested.CategoryId.Value);
                    embed.AddField("Category", $"{parent.Name} (`{parent.Id}`)", inline: true);
                }

                if (channel is IGuildChannel guildChannel)
                {
                    embed.AddField("Server", $"{guildChannel.Guild.Name} (`{guildChannel.GuildId}`)", inline: true);
                    switch (guildChannel)
                    {
                        case IVoiceChannel voice:
                            embed
                                .AddField("Bitrate", $"{voice.Bitrate} bps", inline: true)
                                .AddField("User Limit", voice.UserLimit.HasValue ? $"{voice.UserLimit.Value}" : "None", inline: true);
                            break;

                        case ITextChannel text:
                            embed.AddField("Topic", string.IsNullOrEmpty(text.Topic) ? "None" : text.Topic);
                            break;

                        case ICategoryChannel category:
                            var channels = await category.Guild.GetChannelsAsync();
                            var children = channels.OfType<INestedChannel>()
                                .Where(c => c.CategoryId == category.Id)
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
