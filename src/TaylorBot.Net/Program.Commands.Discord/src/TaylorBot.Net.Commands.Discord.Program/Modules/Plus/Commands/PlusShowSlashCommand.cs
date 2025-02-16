using Discord;
using Humanizer;
using TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Plus.Commands;

public class PlusShowSlashCommand(IPlusRepository plusRepository, IPlusUserRepository plusUserRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "plus show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var embed = new EmbedBuilder();

                var plusUser = await plusUserRepository.GetPlusUserAsync(context.User);

                if (plusUser != null)
                {
                    if (plusUser.IsActive)
                    {
                        embed
                            .WithColor(TaylorBotColors.DiamondBlueColor)
                            .WithDescription(
                                $"""
                                You are currently a **TaylorBot Plus** member 💎
                                Thank you for supporting {"TaylorBot on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}! 💖
                                If you have any questions, join the {"TaylorBot support server".DiscordMdLink("https://discord.gg/3qVNd5P")} 😊
                                """)
                            .AddField(
                                "Plus Servers",
                                plusUser.ActivePlusGuilds.Count > 0 ?
                                    $"""
                                    These servers benefit from **TaylorBot Plus** features thanks to you!
                                    {string.Join('\n', plusUser.ActivePlusGuilds.Select(name => $"- {name}"))}

                                    Use {context.MentionSlashCommand("plus add")} to add plus servers (up to **{plusUser.MaxPlusGuilds}**) 😳
                                    """.Truncate(EmbedFieldBuilder.MaxFieldValueLength) :
                                    $"You don't have any plus server set, add one with {context.MentionSlashCommand("plus add")} (up to **{plusUser.MaxPlusGuilds}**)!"
                            );
                    }
                    else
                    {
                        embed
                            .WithColor(TaylorBotColors.SuccessColor)
                            .WithDescription(
                                $"""
                                You are currently not part of **TaylorBot Plus**, but you have been in the past! 😮
                                Thank you for previously supporting {"TaylorBot on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")}, I hope to see you back one day! 💖
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
                            You are currently not part of **TaylorBot Plus** 😢
                            TaylorBot is free for everyone thanks to the community members that {"support on Patreon".DiscordMdLink("https://www.patreon.com/taylorbot")} 💖
                            If you support on Patreon, you'll also get access to exclusive features and points 😱
                            """);
                }

                if (context.Guild?.Fetched != null)
                {
                    var isPlus = await plusRepository.IsActivePlusGuildAsync(context.Guild);
                    var guild = context.Guild.Fetched;

                    var text = isPlus ?
                        $"'{guild.Name}' is a **TaylorBot Plus** server ✅" :
                        $"""
                        '{guild.Name}' is not a **TaylorBot Plus** server ❌
                        Use {context.MentionSlashCommand("plus add")} to give it access to exclusive perks ➕
                        """;

                    embed.AddField("This Server", text);
                }

                return new EmbedResult(embed.Build());
            }
        ));
    }
}
