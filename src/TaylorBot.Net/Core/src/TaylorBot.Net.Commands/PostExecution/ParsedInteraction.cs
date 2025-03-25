using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands.PostExecution;

public record ParsedInteraction(
    Interaction Raw,
    SnowflakeId Id,
    Interaction.InteractionData Data,
    ParsedInteraction.GuildData? Guild,
    Interaction.User User,
    ParsedInteraction.ChannelData Channel
)
{
    public static ParsedInteraction Parse(Interaction raw, CommandActivity activity)
    {
        ArgumentNullException.ThrowIfNull(raw.data);
        ArgumentNullException.ThrowIfNull(raw.channel_id);
        ArgumentNullException.ThrowIfNull(raw.channel);

        GuildData? guildData = raw.member != null && raw.guild_id != null
            ? new(raw.guild_id, raw.member)
            : null;

        Interaction.User? user = guildData?.Member.user ?? raw.user;
        ArgumentNullException.ThrowIfNull(user);

        ParsedInteraction parsed = new(
            raw,
            raw.id,
            raw.data,
            guildData,
            user,
            new(raw.channel_id, raw.channel));

        activity.SetCommandName(parsed.Data.name);
        activity.SetUserId(parsed.UserId);
        activity.SetChannelId(parsed.Channel.Id);
        activity.SetGuildId(parsed.Guild?.Id);

        return parsed;
    }

    public string Token => Raw.token;

    public SnowflakeId UserId => User.id;

    public record GuildData(SnowflakeId Id, Interaction.Member Member);

    public record ChannelData(SnowflakeId Id, Interaction.PartialChannel Partial);
}
