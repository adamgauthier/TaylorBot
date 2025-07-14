using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

[Name("DiscordInfo 💬")]
public class DiscordInfoModule(ICommandRunner commandRunner, PrefixedCommandRunner prefixedCommandRunner, AvatarSlashCommand avatarCommand) : TaylorBotModule
{
    [Command("avatar")]
    [Alias("av", "avi")]
    public async Task<RuntimeResult> AvatarAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: AvatarSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            avatarCommand.Avatar(new(u), AvatarType.Guild, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("userinfo")]
    [Alias("uinfo", "randomuserinfo", "randomuser", "randomuinfo")]
    public async Task<RuntimeResult> UserInfoAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: InspectUserSlashCommand.CommandName, IsRemoved: true));

    [Command("roleinfo")]
    [Alias("rinfo")]
    public async Task<RuntimeResult> RoleInfoAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: InspectRoleSlashCommand.CommandName, IsRemoved: true));

    [Command("channelinfo")]
    [Alias("cinfo")]
    public async Task<RuntimeResult> ChannelInfoAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: InspectChannelSlashCommand.CommandName, IsRemoved: true));

    [Command("serverinfo")]
    [Alias("sinfo", "guildinfo", "ginfo")]
    public async Task<RuntimeResult> ServerInfoAsync() => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: InspectServerSlashCommand.CommandName, IsRemoved: true));
}
