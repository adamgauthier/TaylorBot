using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

[Name("Favorite")]
public class FavoriteModule(
    ICommandRunner commandRunner,
    FavoriteSongsShowSlashCommand favoriteSongsShowCommand,
    FavoriteSongsSetSlashCommand favoriteSongsSetCommand,
    FavoriteBaeShowSlashCommand favoriteBaeShowCommand,
    FavoriteObsessionShowSlashCommand favoriteObsessionShowCommand,
    PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command(FavoriteSongsShowSlashCommand.PrefixCommandName)]
    [Alias(FavoriteSongsShowSlashCommand.PrefixCommandAlias1, FavoriteSongsShowSlashCommand.PrefixCommandAlias2)]
    public async Task<RuntimeResult> ShowFavAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: FavoriteSongsShowSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteSongsShowCommand.Show(new(u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command(FavoriteSongsSetSlashCommand.PrefixCommandName)]
    [Alias(FavoriteSongsSetSlashCommand.PrefixCommandAlias1, FavoriteSongsSetSlashCommand.PrefixCommandAlias2, FavoriteSongsSetSlashCommand.PrefixCommandAlias3)]
    public async Task<RuntimeResult> SetFavAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: FavoriteSongsSetSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteSongsSetCommand.SetCommand(context.User, text, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("clearfav")]
    [Alias("clear fav", "clearfavsongs", "clear favsongs", "clearfavoritesongs", "clear favoritesongs")]
    public async Task<RuntimeResult> ClearSongsAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: FavoriteSongsClearSlashCommand.CommandName, IsRemoved: true));

    [Command(FavoriteBaeShowSlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> ShowBaeAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: FavoriteBaeShowSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteBaeShowCommand.Show(new(u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setbae")]
    [Alias("set bae")]
    public async Task<RuntimeResult> SetBaeAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: FavoriteBaeSetSlashCommand.CommandName, IsRemoved: true));

    [Command("clearbae")]
    public async Task<RuntimeResult> ClearBaeAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: FavoriteBaeClearSlashCommand.CommandName, IsRemoved: true));

    [Command(FavoriteObsessionShowSlashCommand.PrefixCommandName)]
    public async Task<RuntimeResult> ShowWaifuAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: FavoriteObsessionShowSlashCommand.CommandName));
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteObsessionShowCommand.Show(new(u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setwaifu")]
    [Alias("set waifu")]
    public async Task<RuntimeResult> SetWaifuAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: FavoriteObsessionSetSlashCommand.CommandName, IsRemoved: true));

    [Command("clearwaifu")]
    [Alias("clear waifu")]
    public async Task<RuntimeResult> ClearWaifuAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: FavoriteObsessionClearSlashCommand.CommandName, IsRemoved: true));
}
