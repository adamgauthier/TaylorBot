using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Commands.Types;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Commands;

[Name("Knowledge ❓")]
public class KnowledgeModule : TaylorBotModule
{
    private readonly ICommandRunner _commandRunner;
    private readonly UrbanDictionaryCommand _urbanDictionaryCommand;

    public KnowledgeModule(ICommandRunner commandRunner, UrbanDictionaryCommand urbanDictionaryCommand)
    {
        _commandRunner = commandRunner;
        _urbanDictionaryCommand = urbanDictionaryCommand;
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

    [Command("urbandictionary")]
    [Alias("urban")]
    [Summary("Get definitions for slang words and phrases with UrbanDictionary.")]
    public async Task<RuntimeResult> UrbanDictionaryAsync(
        [Remainder]
        string text
    )
    {
        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(
            _urbanDictionaryCommand.Search(Context.User, text, isLegacyCommand: true),
            context
        );

        return new TaylorBotResult(result, context);
    }
}
