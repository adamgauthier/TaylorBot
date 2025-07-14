using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.UserLocation.Commands;

[Name("Location 🌍")]
public class LocationModule(ICommandRunner commandRunner, WeatherSlashCommand weatherCommand, LocationShowCommand locationShowCommand, PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("weather")]
    public async Task<RuntimeResult> WeatherAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        IUser u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: WeatherSlashCommand.CommandName));

        var result = await commandRunner.RunSlashCommandAsync(
            weatherCommand.Weather(context.User, new(u), locationOverride: null, context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("location")]
    [Alias("time")]
    public async Task<RuntimeResult> LocationAsync(
        [Remainder]
        IUserArgument<IUser>? user = null
    )
    {
        IUser u = user == null ?
            Context.User :
            await user.GetTrackedUserAsync();

        var context = DiscordNetContextMapper.MapToRunContext(Context, new(ReplacementSlashCommand: LocationShowSlashCommand.CommandName));

        var result = await commandRunner.RunSlashCommandAsync(
            locationShowCommand.Location(new(u), context),
            context
        );

        return new TaylorBotResult(result, context);
    }

    [Command("weatherat")]
    public async Task<RuntimeResult> WeatherAtAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: WeatherSlashCommand.CommandName, IsRemoved: true));
}
