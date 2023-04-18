using System.Collections.Generic;

namespace TaylorBot.Net.Core.Client;

public record Interaction(
    string id,
    byte type,
    string token,
    Interaction.ApplicationCommandInteractionData? data,
    Interaction.GuildMember? member,
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

    public record GuildMember(User user, string permissions, string joined_at);

    public record User(string id, string username, string discriminator);

    public record Message(string id);

    public record Resolved(IReadOnlyDictionary<string, Attachment>? attachments);

    public record Attachment(string id, string filename, string url, int size, string content_type);
}
