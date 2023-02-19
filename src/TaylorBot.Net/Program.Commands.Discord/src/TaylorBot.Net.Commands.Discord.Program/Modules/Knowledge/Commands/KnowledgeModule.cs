using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Commands
{
    [Name("Knowledge ❓")]
    public class KnowledgeModule : TaylorBotModule
    {
        private readonly ICommandRunner _commandRunner;

        public KnowledgeModule(ICommandRunner commandRunner)
        {
            _commandRunner = commandRunner;
        }

        [Command("horoscope")]
        [Alias("hs")]
        [Summary("Gets the horoscope of a user based on their set birthday.")]
        public async Task<RuntimeResult> HoroscopeAsync(
            [Summary("What user would you like to see the horoscope of?")]
            [Remainder]
            IUserArgument<IUser>? user = null
        )
        {
            var command = new Command(
                DiscordNetContextMapper.MapToCommandMetadata(Context),
                () => new(new EmbedResult(
                    EmbedFactory.CreateError($"This command has been moved to </birthday horoscope:1016938623880400907>. Please use it instead! 😊")
                ))
            );

            var context = DiscordNetContextMapper.MapToRunContext(Context);
            var result = await _commandRunner.RunAsync(command, context);

            return new TaylorBotResult(result, context);
        }
    }
}
