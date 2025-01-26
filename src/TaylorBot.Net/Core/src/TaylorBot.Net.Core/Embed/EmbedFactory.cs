using Discord;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Core.Embed;

public static class EmbedFactory
{
    public static DiscordEmbed CreateSuccessEmbed(string description) =>
        new(color: TaylorBotColors.SuccessColor.RawValue, description: description);

    public static DiscordEmbed CreateErrorEmbed(string description) =>
        new(color: TaylorBotColors.ErrorColor.RawValue, description: description);

    public static Discord.Embed CreateSuccess(string description) =>
        new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor).WithDescription(description).Build();

    public static Discord.Embed CreateWarning(string description) =>
        new EmbedBuilder().WithColor(TaylorBotColors.WarningColor).WithDescription(description).Build();

    public static Discord.Embed CreateError(string description) =>
        new EmbedBuilder().WithColor(TaylorBotColors.ErrorColor).WithDescription(description).Build();
}
