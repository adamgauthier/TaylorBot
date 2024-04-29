using Discord;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Core.Client;

public record Interaction(
    string id,
    byte type,
    string token,
    Interaction.ApplicationCommandInteractionData? data,
    Interaction.Member? member,
    Interaction.User? user,
    string? guild_id,
    string? channel_id,
    Interaction.Message? message
)
{
    public record ApplicationCommandInteractionData(
        string id,
        string name,
        IReadOnlyList<ApplicationCommandOption>? options,
        string? custom_id,
        byte? component_type,
        IReadOnlyList<ApplicationCommandComponent>? components,
        Resolved? resolved
    );

    public record ApplicationCommandOption(string name, byte type, object? value, IReadOnlyList<ApplicationCommandOption>? options);

    public record ApplicationCommandComponent(byte type, string? custom_id, string? value, IReadOnlyList<ApplicationCommandComponent>? components);

    public record PartialMember(string permissions, string joined_at, string? avatar, IReadOnlyList<string> roles);

    public record Member(User user, string permissions, string joined_at, string? avatar, IReadOnlyList<string> roles);

    public record User(string id, string username, string discriminator, string? avatar, bool? bot);

    public record Message(string id);

    public record Resolved(IReadOnlyDictionary<string, User>? users, IReadOnlyDictionary<string, PartialMember>? members, IReadOnlyDictionary<string, Attachment>? attachments);

    public record Attachment(string id, string filename, string url, int size, string content_type);
}

public interface IInteraction
{
    string Token { get; }
}

public class InteractionMapper
{
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
        return DateTimeOffset.Parse(joined_at);
    }

    private static List<SnowflakeId> ParseRoleIds(IReadOnlyList<string> roles)
    {
        return roles.Select(r => new SnowflakeId(r)).ToList();
    }
}
