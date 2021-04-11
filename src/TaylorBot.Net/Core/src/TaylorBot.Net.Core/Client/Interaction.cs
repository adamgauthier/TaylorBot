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
        string? channel_id
    )
    {
        public record ApplicationCommandInteractionData(string id, string name, IReadOnlyList<ApplicationCommandInteractionDataOption>? options);

        public record ApplicationCommandInteractionDataOption(string name, object? value);

        public record GuildMember(User user);

        public record User(string id);
    }
}
