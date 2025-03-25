using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

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
        var result = await commandRunner.RunSlashCommandAsync(
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
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect user:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

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
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect role:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

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
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect channel:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("serverinfo")]
    [Alias("sinfo", "guildinfo", "ginfo")]
    [Summary("This command has been moved to </inspect server:1260489511297749054>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ServerInfoAsync()
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </inspect server:1260489511297749054> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
