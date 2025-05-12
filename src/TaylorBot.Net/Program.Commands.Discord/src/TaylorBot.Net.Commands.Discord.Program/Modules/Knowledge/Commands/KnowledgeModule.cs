using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Commands;

[Name("Knowledge ❓")]
public class KnowledgeModule(ICommandRunner commandRunner) : TaylorBotModule
{
    [Command("horoscope")]
    [Alias("hs")]
    [Summary("This command has been moved to </birthday horoscope:1016938623880400907>. Please use it instead! 😊")]
    public async Task<RuntimeResult> HoroscopeAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(
                EmbedFactory.CreateError("This command has been moved to </birthday horoscope:1016938623880400907>. Please use it instead! 😊")
            )));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command("urbandictionary")]
    [Alias("urban")]
    [Summary("This command has been moved to 👉 </urbandictionary:1080373890020282508> 👈 Please use it instead! 😊")]
    public async Task<RuntimeResult> UrbanDictionaryAsync(
        [Remainder]
        string? _ = null)
    {
        Command command = new(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            () => new(new EmbedResult(
                EmbedFactory.CreateError("This command has been moved to </urbandictionary:1080373890020282508>. Please use it instead! 😊")
            )));

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
