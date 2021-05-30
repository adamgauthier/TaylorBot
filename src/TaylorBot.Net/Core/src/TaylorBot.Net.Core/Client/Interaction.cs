using System.Collections.Generic;

namespace TaylorBot.Net.Core.Client
{
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
        public record ApplicationCommandInteractionData(string id, string name, IReadOnlyList<ApplicationCommandInteractionDataOption>? options, string? custom_id, byte? component_type);

        public record ApplicationCommandInteractionDataOption(string name, byte type, object? value, IReadOnlyList<ApplicationCommandInteractionDataOption>? options);

        public record GuildMember(User user);

        public record User(string id);

        public record Message(string id);
    }
}
