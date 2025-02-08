using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

[Name("Favorite")]
public class FavoriteModule(
    ICommandRunner commandRunner,
    FavoriteSongsShowSlashCommand favoriteSongsShowCommand,
    FavoriteSongsSetSlashCommand favoriteSongsSetCommand,
    FavoriteBaeShowSlashCommand favoriteBaeShowCommand,
    FavoriteObsessionShowSlashCommand favoriteObsessionShowCommand) : TaylorBotModule
{
    [Command(FavoriteSongsShowSlashCommand.PrefixCommandName)]
    [Alias(FavoriteSongsShowSlashCommand.PrefixCommandAlias1, FavoriteSongsShowSlashCommand.PrefixCommandAlias2)]
    [Summary("Show the favorite songs of a user")]
    public async Task<RuntimeResult> ShowFavAsync(
        [Summary("What user would you like to see the favorite songs of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteSongsShowCommand.Show(new(u)),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command(FavoriteSongsSetSlashCommand.PrefixCommandName)]
    [Alias(FavoriteSongsSetSlashCommand.PrefixCommandAlias1, FavoriteSongsSetSlashCommand.PrefixCommandAlias2, FavoriteSongsSetSlashCommand.PrefixCommandAlias3)]
    [Summary("Register your favorite songs for others to see")]
    public async Task<RuntimeResult> SetFavAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteSongsSetCommand.Set(context.User, text, null),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("clearfav")]
    [Alias("clear fav", "clearfavsongs", "clear favsongs", "clearfavoritesongs", "clear favoritesongs")]
    [Summary("This command has been moved to </favorite songs clear:1169468169140838502>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearFavAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </favorite songs clear:1169468169140838502> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command(FavoriteBaeShowSlashCommand.PrefixCommandName)]
    [Summary("Show the bae of a user")]
    public async Task<RuntimeResult> ShowBaeAsync(
        [Summary("What user would you like to see the bae of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteBaeShowCommand.Show(new(u)),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setbae")]
    [Alias("set bae")]
    [Summary("This command has been moved to </favorite bae set:1169468169140838502>. Please use it instead! 😊")]
    public async Task<RuntimeResult> SetBaeAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </favorite bae set:1169468169140838502> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clearbae")]
    [Summary("This command has been moved to </favorite bae clear:1169468169140838502>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearBaeAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </favorite bae clear:1169468169140838502> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command(FavoriteObsessionShowSlashCommand.PrefixCommandName)]
    [Summary("Show the obsession of a user")]
    public async Task<RuntimeResult> ShowWaifuAsync(
        [Summary("What user would you like to see the obsession of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(
            favoriteObsessionShowCommand.Show(new(u)),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setwaifu")]
    [Alias("set waifu")]
    [Summary("This command has been moved to </favorite obsession set:1169468169140838502>. Please use it instead! 😊")]
    public async Task<RuntimeResult> SetWaifuAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </favorite obsession set:1169468169140838502> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("clearwaifu")]
    [Alias("clear waifu")]
    [Summary("This command has been moved to </favorite obsession clear:1169468169140838502>. Please use it instead! 😊")]
    public async Task<RuntimeResult> ClearWaifuAsync(
        [Remainder]
        string? _ = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </favorite obsession clear:1169468169140838502> 👈
                Please use it instead! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
