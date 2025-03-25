using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

[Name("Location 🌍")]
public class LocationModule(ICommandRunner commandRunner, WeatherCommand weatherCommand, LocationShowCommand locationShowCommand) : TaylorBotModule
{
    [Command("weather")]
    [Summary("Gets current weather forecast for a user's location. Icons by Dr. Lex.")]
    public async Task<RuntimeResult> WeatherAsync(
        [Summary("What user would you like to see the weather for?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        IUser u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);

        var result = await commandRunner.RunSlashCommandAsync(
            weatherCommand.Weather(context.User, new(u), locationOverride: null),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("location")]
    [Alias("time")]
    [Summary("Gets set location for a user.")]
    public async Task<RuntimeResult> LocationAsync(
        [Summary("What user would you like to see the location for?")]
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        IUser u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context);

        var result = await commandRunner.RunSlashCommandAsync(
            locationShowCommand.Location(new(u)),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("weatherat")]
    [Summary("This command has been moved to </location weather:1141925890448691270>. Please use it instead with the **location** option! 😊")]
    public async Task<RuntimeResult> RpsWinsAsync(
        [Remainder]
        string? _ = null
    )
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(EmbedFactory.CreateError(
                """
                This command has been moved to 👉 </location weather:1141925890448691270> 👈
                Please use it instead with the **location** option! 😊
                """))));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
