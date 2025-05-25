﻿using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Stats.Commands;

[Name("Stats 📊")]
public class StatsModule(ICommandRunner commandRunner, IBotInfoRepository botInfoRepository) : TaylorBotModule
{
    [Command("serverstats")]
    [Alias("sstats", "genderstats", "agestats")]
    [Summary("Gets age and gender stats for a server.")]
    public async Task<RuntimeResult> ServerStatsAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </server population:1137547317549998130> 👈
                Please use it instead! 😊
                """
            )))
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("botinfo")]
    [Alias("version", "invite")]
    [Summary("Gets general information about TaylorBot.")]
    public async Task<RuntimeResult> BotInfoAsync()
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var productVersion = await botInfoRepository.GetProductVersionAsync();
            var applicationInfo = await Context.Client.GetApplicationInfoAsync();

            return new EmbedResult(new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(applicationInfo.Description)
                .AddField("Version", productVersion, inline: true)
                .AddField("Author", applicationInfo.Owner.Mention, inline: true)
                .AddField("Invite Link", "https://taylorbot.app/", inline: true)
            .Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
