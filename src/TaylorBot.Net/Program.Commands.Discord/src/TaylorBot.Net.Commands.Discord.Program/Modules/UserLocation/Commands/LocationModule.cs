using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

[Name("Location 🌍")]
public class LocationModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly WeatherCommand _weatherCommand;
    private readonly LocationShowCommand _locationShowCommand;

    public LocationModule(ICommandRunner commandRunner, WeatherCommand weatherCommand, LocationShowCommand locationShowCommand)
    {
        _commandRunner = commandRunner;
        _weatherCommand = weatherCommand;
        _locationShowCommand = locationShowCommand;
    }

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

        var result = await _commandRunner.RunAsync(
            _weatherCommand.Weather(context.User, u, locationOverride: null, Context.CommandPrefix),
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

        var result = await _commandRunner.RunAsync(
            _locationShowCommand.Location(u),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
