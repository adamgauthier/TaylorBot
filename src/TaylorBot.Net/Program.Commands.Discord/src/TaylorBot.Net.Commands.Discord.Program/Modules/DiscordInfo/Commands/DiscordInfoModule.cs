using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Time;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

[Name("DiscordInfo 💬")]
public class DiscordInfoModule(ICommandRunner commandRunner, AvatarSlashCommand avatarCommand) : TaylorBotModule
{
    [Command("avatar")]
    [Alias("av", "avi")]
    [Summary("Displays the avatar of a user.")]
    public async Task<RuntimeResult> AvatarAsync(
        [Summary("What user would you like to see the avatar of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(
            avatarCommand.Avatar(new(u), AvatarType.Guild, "Use </avatar:832103922709692436> instead! 😊"),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("userinfo")]
    [Alias("uinfo", "randomuserinfo", "randomuser", "randomuinfo")]
    [Summary("This command has been moved to </inspect user:1260489511297749054>. Please use it instead! 😊")]
    public async Task<RuntimeResult> UserInfoAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect user:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("roleinfo")]
    [Alias("rinfo")]
    [Summary("This command has been moved to </inspect role:1260489511297749054>. Please use it instead! 😊")]
    public async Task<RuntimeResult> RoleInfoAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect role:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("channelinfo")]
    [Alias("cinfo")]
    [Summary("This command has been moved to </inspect channel:1260489511297749054>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ChannelInfoAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect channel:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("serverinfo")]
    [Alias("sinfo", "guildinfo", "ginfo")]
    [Summary("Gets discord information about the current server.")]
    public async Task<RuntimeResult> ServerInfoAsync()
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                var guild = Context.Guild;
                var channels = await guild.GetChannelsAsync();
                var orderedRoles = guild.Roles.OrderByDescending(r => r.Position).ToList();

                var embed = new EmbedBuilder()
                    .WithGuildAsAuthor(guild)
                    .WithColor(orderedRoles.First().Color)
                    .AddField("Id", $"`{guild.Id}`", inline: true)
                    .AddField("Owner", MentionUtils.MentionUser(guild.OwnerId), inline: true)
                    .AddField("Members", guild is SocketGuild socketGuild ? $"{socketGuild.MemberCount}" : "?", inline: true)
                    .AddField("Boosts", guild.PremiumSubscriptionCount, inline: true)
                    .AddField("Custom Emotes", guild.Emotes.Count, inline: true)
                    .AddField("Custom Stickers", guild.Stickers.Count, inline: true)
                    .AddField("Created", guild.CreatedAt.FormatFullUserDate(TaylorBotCulture.Culture));

                if (!string.IsNullOrWhiteSpace(guild.Description))
                {
                    embed.AddField("Description", guild.Description);
                }

                if (guild.BannerUrl != null)
                {
                    embed.WithImageUrl(guild.BannerUrl);
                }

                embed
                    .AddField(
                        "Channel".ToQuantity(channels.Count),
                        $"{channels.OfType<ICategoryChannel>().Count()} Category, {channels.OfType<ITextChannel>().Count()} Text ({channels.OfType<IThreadChannel>().Count()} Thread), {channels.OfType<IVoiceChannel>().Count()} Voice ({channels.OfType<IStageChannel>().Count()} Stage)"
                    )
                    .AddField(
                        "Role".ToQuantity(orderedRoles.Count),
                        string.Join(", ", orderedRoles.Take(4).Select(r => r.Mention)) + (orderedRoles.Count > 4 ? ", ..." : string.Empty)
                    );

                if (guild.IconUrl != null)
                {
                    embed.WithThumbnailUrl(guild.IconUrl);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [new InGuildPrecondition()]
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
