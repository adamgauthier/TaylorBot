using Discord;
using System.Globalization;
using System.Text.Json;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Core.Client;

public record Interaction(
    string id,
    byte type,
    string token,
    Interaction.InteractionData? data,
    Interaction.Member? member,
    Interaction.User? user,
    string? guild_id,
    string? channel_id,
    Interaction.PartialChannel? channel,
    Interaction.Message? message
)
{
    public record InteractionData(
        // Number for MESSAGE_COMPONENT, String for APPLICATION_COMMAND
        JsonElement? id,
        string? name,
        IReadOnlyList<ApplicationCommandOption>? options,
        string? custom_id,
        byte? component_type,
        IReadOnlyList<string>? values,
        Resolved? resolved,
        IReadOnlyList<ApplicationCommandComponent>? components
    );

    public record ApplicationCommandOption(string name, byte type, object? value, IReadOnlyList<ApplicationCommandOption>? options);

    public record ApplicationCommandComponent(byte type, string? custom_id, string? value, IReadOnlyList<ApplicationCommandComponent>? components);

    public record PartialMember(string permissions, string joined_at, string? avatar, IReadOnlyList<string> roles);

    public record Member(User user, string permissions, string joined_at, string? avatar, IReadOnlyList<string> roles);

    public record User(string id, string username, string discriminator, string? avatar, bool? bot);

    public record Message(string id, DateTimeOffset timestamp, IReadOnlyList<DiscordEmbed> embeds, IReadOnlyList<Component>? components = null, InteractionMetadata? interaction_metadata = null);

    public record InteractionMetadata(User user, string? name = null);

    public record Component(string? custom_id = null, IReadOnlyList<Component>? components = null);

    public record Resolved(
        IReadOnlyDictionary<string, User>? users,
        IReadOnlyDictionary<string, PartialMember>? members,
        IReadOnlyDictionary<string, DiscordRole>? roles,
        IReadOnlyDictionary<string, PartialChannel>? channels,
        IReadOnlyDictionary<string, Attachment>? attachments);

    public record PartialChannel(string id, byte type, string? guild_id);

    public record Attachment(string id, string filename, string url, int size, string content_type);
}

public record DiscordEmbed(
    string? title = null,
    string? description = null,
    string? url = null,
    DiscordEmbed.EmbedAuthor? author = null,
    DiscordEmbed.EmbedImage? image = null,
    DiscordEmbed.EmbedThumbnail? thumbnail = null,
    uint? color = null,
    DiscordEmbed.EmbedFooter? footer = null,
    IReadOnlyList<DiscordEmbed.EmbedField>? fields = null,
    string? timestamp = null)
{
    public record EmbedAuthor(string? name, string? url, string? icon_url);

    public record EmbedImage(string url);

    public record EmbedThumbnail(string url);

    public record EmbedFooter(string text, string? icon_url, string? proxy_icon_url);

    public record EmbedField(string name, string value, bool? inline);
}

public record DiscordRole(
    SnowflakeId id,
    string name,
    uint color,
    bool hoist,
    string? icon,
    string? unicode_emoji,
    int position,
    string permissions,
    bool managed,
    bool mentionable,
    RoleTags? tags,
    int flags
);

public record RoleTags(
    string? bot_id,
    string? integration_id,
    bool? premium_subscriber,
    string? subscription_listing_id,
    bool? available_for_purchase,
    bool? guild_connections
);

public class InteractionMapper
{
    public DiscordUser ToUser(Interaction.User user, Interaction.PartialMember? member = null)
    {
        return new(
            user.id,
            user.username,
            user.avatar,
            user.discriminator,
            IsBot: user.bot == true,
            MemberInfo: member != null ? ToMemberInfo(new SnowflakeId(member.permissions), member) : null);
    }

    public DiscordMemberInfo ToMemberInfo(SnowflakeId guildId, Interaction.PartialMember member)
    {
        return new(
            guildId,
            ParseDate(member.joined_at),
            ParseRoleIds(member.roles),
            new GuildPermissions(member.permissions),
            member.avatar);
    }

    public DiscordMemberInfo ToMemberInfo(SnowflakeId guildId, Interaction.Member member)
    {
        return new(
            guildId,
            ParseDate(member.joined_at),
            ParseRoleIds(member.roles),
            new GuildPermissions(member.permissions),
            member.avatar);
    }

    private static DateTimeOffset ParseDate(string joined_at)
    {
        return DateTimeOffset.Parse(joined_at, CultureInfo.InvariantCulture);
    }

    private static List<SnowflakeId> ParseRoleIds(IReadOnlyList<string> roles)
    {
        return [.. roles.Select(r => new SnowflakeId(r))];
    }

    public static DiscordEmbed ToInteractionEmbed(Discord.Embed embed)
    {
        return new DiscordEmbed(
            title: embed.Title,
            description: embed.Description,
            url: embed.Url,
            author: embed.Author.HasValue ? new(embed.Author.Value.Name, embed.Author.Value.Url, embed.Author.Value.IconUrl) : null,
            image: embed.Image.HasValue ? new(embed.Image.Value.Url) : null,
            thumbnail: embed.Thumbnail.HasValue ? new(embed.Thumbnail.Value.Url) : null,
            color: embed.Color.HasValue ? embed.Color.Value.RawValue : null,
            footer: embed.Footer.HasValue ? new(embed.Footer.Value.Text, embed.Footer.Value.IconUrl, embed.Footer.Value.ProxyUrl) : null,
            fields: [.. embed.Fields.Select(f => new DiscordEmbed.EmbedField(f.Name, f.Value, f.Inline))],
            timestamp: embed.Timestamp.HasValue ? embed.Timestamp.Value.ToString("s", CultureInfo.InvariantCulture) : null
        );
    }

    public static Discord.Embed ToDiscordEmbed(DiscordEmbed embed)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = embed.title,
            Description = embed.description,
            Url = embed.url,
            Color = embed.color.HasValue ? new Discord.Color(embed.color.Value) : null,
            Timestamp = embed.timestamp != null ? DateTimeOffset.Parse(embed.timestamp, CultureInfo.InvariantCulture) : null,
        };

        if (embed.author != null)
        {
            embedBuilder.Author = new EmbedAuthorBuilder
            {
                Name = embed.author.name,
                Url = embed.author.url,
                IconUrl = embed.author.icon_url,
            };
        }

        if (embed.image != null)
        {
            embedBuilder.ImageUrl = embed.image.url;
        }

        if (embed.thumbnail != null)
        {
            embedBuilder.ThumbnailUrl = embed.thumbnail.url;
        }

        if (embed.footer != null)
        {
            embedBuilder.Footer = new EmbedFooterBuilder
            {
                Text = embed.footer.text,
                IconUrl = embed.footer.icon_url,
            };
        }

        if (embed.fields != null)
        {
            foreach (var field in embed.fields)
            {
                embedBuilder.AddField(field.name, field.value, field.inline ?? false);
            }
        }

        return embedBuilder.Build();
    }
}
