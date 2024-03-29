﻿using Discord;
using Discord.Commands;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

[Name("TaylorBot Plus 💎")]
[Group("plus")]
[Alias("support", "patreon", "donate")]
public class PlusModule(ICommandRunner commandRunner, IPlusRepository plusRepository, IPlusUserRepository plusUserRepository) : TaylorBotModule
{
    [Priority(-1)]
    [Command]
    [Summary("Gets basic information about your TaylorBot Plus membership.")]
    public async Task<RuntimeResult> PlusAsync()
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var embed = new EmbedBuilder();

            var plusUser = await plusUserRepository.GetPlusUserAsync(Context.User);

            if (plusUser != null)
            {
                if (plusUser.IsActive)
                {
                    embed
                        .WithColor(TaylorBotColors.DiamondBlueColor)
                        .WithDescription(
                            $"""
                            You are currently a **TaylorBot Plus** member. 💎
                            Thank you for supporting {"TaylorBot on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}! ♥
                            If you have any questions, join the TaylorBot server listed on the Patreon. 😊
                            """)
                        .AddField("Plus Servers", Context.Channel is ITextChannel ?
                            "Your plus servers are hidden for privacy reasons, type `plus` in TaylorBot DMs to see them! 🕵" :
                            plusUser.ActivePlusGuilds.Count != 0 ?
                                string.Join('\n',
                                    new[] { $"These servers benefit from **TaylorBot Plus** features thanks to you! You can add more with `plus add`, up to **{plusUser.MaxPlusGuilds}**:" }
                                    .Concat(plusUser.ActivePlusGuilds.Select(name => $"- {name}"))
                                ).Truncate(EmbedFieldBuilder.MaxFieldValueLength) :
                                $"You don't have any plus server set, add one with `plus add` (up to **{plusUser.MaxPlusGuilds}**)!"
                        );
                }
                else
                {
                    embed
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription(
                            $"""
                            You are currently not part of **TaylorBot Plus**, but you have been in the past! 😮
                            Thank you for previously supporting {"TaylorBot on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}, I hope to see you back one day! ♥
                            """);
                }
            }
            else
            {
                embed
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithTitle("Support TaylorBot 🥺")
                    .WithUrl("https://www.patreon.com/taylorbot")
                    .WithThumbnailUrl("https://i.imgur.com/55CptF4.jpg")
                    .WithDescription(
                        $"""
                        You are currently not part of **TaylorBot Plus**. 🚫
                        TaylorBot is free for everyone thanks to the community members that {"support me on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}. ♥
                        If you choose to pledge to the Patreon, you'll also get access to TaylorBot Plus features and points. 😱
                        """);
            }

            if (Context.Guild != null)
            {
                var isPlus = await plusRepository.IsActivePlusGuildAsync(Context.Guild);

                var text = isPlus ?
                    $"'{Context.Guild.Name}' is a **TaylorBot Plus** server. ✅" :
                    $"""
                    '{Context.Guild.Name}' is not a **TaylorBot Plus** server. ❌
                    Members with a plus membership can add it using `{Context.CommandPrefix}plus add`.
                    """;

                embed.AddField("This Server", text);
            }

            return new EmbedResult(embed.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("add")]
    [Summary("Adds this server to your TaylorBot Plus servers, giving it access to exclusive features.")]
    public async Task<RuntimeResult> AddAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var plusUser = (await plusUserRepository.GetPlusUserAsync(Context.User))!;

                var embed = new EmbedBuilder();

                if (plusUser.ActivePlusGuilds.Count + 1 > plusUser.MaxPlusGuilds)
                {
                    embed
                        .WithColor(TaylorBotColors.ErrorColor)
                        .WithDescription(
                            $"""
                            Unfortunately you can't add more **TaylorBot Plus** servers with your current membership. 😕
                            Use `{Context.CommandPrefix}plus` to see your plus servers and maybe remove some with `{Context.CommandPrefix}plus remove`
                            """);
                }
                else
                {
                    await plusUserRepository.AddPlusGuildAsync(Context.User, Context.Guild);

                    embed
                        .WithColor(TaylorBotColors.DiamondBlueColor)
                        .WithThumbnailUrl(Context.Guild.IconUrl)
                        .WithDescription(
                            $"""
                            Successfully added {Context.Guild.Name} to your plus servers. 😊
                            It should now have access to **TaylorBot Plus** features. 💎
                            """);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [
                new InGuildPrecondition(),
                new PlusPrecondition(plusRepository, PlusRequirement.PlusUser)
            ]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("remove")]
    [Summary("Removes this server from your TaylorBot Plus servers, losing access to exclusive features.")]
    public async Task<RuntimeResult> RemoveAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                await plusUserRepository.DisablePlusGuildAsync(Context.User, Context.Guild);

                return new EmbedResult(new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithDescription(
                        $"""
                        Successfully removed {Context.Guild.Name} from your plus servers. 😊
                        This server will lose access to **TaylorBot Plus** features, use `{Context.CommandPrefix}plus add` if you change your mind.
                        """)
                    .Build()
                );
            },
            Preconditions: [
                new InGuildPrecondition(),
                new PlusPrecondition(plusRepository, PlusRequirement.PlusUser)
            ]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("addsupporterserver")]
    [Alias("ass")]
    [Summary("This command has been moved to `plus add`. Please use it instead! 😊")]
    public async Task<RuntimeResult> AddSupporterServerAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 `plus add` 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("removesupporterserver")]
    [Alias("rss")]
    [Summary("This command has been moved to `plus remove`. Please use it instead! 😊")]
    public async Task<RuntimeResult> RemoveSupporterServerAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 `plus remove` 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
