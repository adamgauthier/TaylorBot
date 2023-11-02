using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Favorite.Commands;

[Name("Favorite")]
public class FavoriteModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly FavoriteSongsShowSlashCommand _favoriteSongsShowCommand;
    private readonly FavoriteSongsSetSlashCommand _favoriteSongsSetCommand;
    private readonly FavoriteBaeShowSlashCommand _favoriteBaeShowCommand;

    public FavoriteModule(ICommandRunner commandRunner, FavoriteSongsShowSlashCommand favoriteSongsShowCommand, FavoriteSongsSetSlashCommand favoriteSongsSetCommand, FavoriteBaeShowSlashCommand favoriteBaeShowCommand)
    {
        _commandRunner = commandRunner;
        _favoriteSongsShowCommand = favoriteSongsShowCommand;
        _favoriteSongsSetCommand = favoriteSongsSetCommand;
        _favoriteBaeShowCommand = favoriteBaeShowCommand;
    }

    [Command("fav")]
    [Alias("favsongs", "favoritesongs")]
    [Summary("Show the favorite songs of a user")]
    public async Task<RuntimeResult> ShowFavAsync(
        [Summary("What user would you like to see the favorite songs of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _favoriteSongsShowCommand.Show(u),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setfav")]
    [Alias("set fav", "setfavsongs", "set favsongs")]
    [Summary("Register your favorite songs for others to see")]
    public async Task<RuntimeResult> SetFavAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _favoriteSongsSetCommand.Set(Context.User, text, null),
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
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("bae")]
    [Summary("Show the bae of a user")]
    public async Task<RuntimeResult> ShowBaeAsync(
        [Summary("What user would you like to see the bae of?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        var u = user == null ? Context.User : await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _favoriteBaeShowCommand.Show(u),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("setbae")]
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
        var result = await _commandRunner.RunAsync(command, context);

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
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
