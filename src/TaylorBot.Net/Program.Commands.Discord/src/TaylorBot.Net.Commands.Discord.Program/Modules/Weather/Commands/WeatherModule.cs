using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Weather.Commands
{
    [Name("Weather 🌦")]
    public class WeatherModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;
        private readonly WeatherCommand _weatherCommand;

        public WeatherModule(ICommandRunner commandRunner, WeatherCommand weatherCommand)
        {
            _commandRunner = commandRunner;
            _weatherCommand = weatherCommand;
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
    }
}
