using Discord.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.Birthday.Commands;
using TaylorBot.Net.Commands.Discord.Program.Modules.UrbanDictionary.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Knowledge.Commands;

[Name("Knowledge ❓")]
public class KnowledgeModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("horoscope")]
    [Alias("hs")]
    public async Task<RuntimeResult> HoroscopeAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: BirthdayHoroscopeSlashCommand.CommandName, IsRemoved: true));

    [Command("urbandictionary")]
    [Alias("urban")]
    public async Task<RuntimeResult> UrbanDictionaryAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(ReplacementSlashCommand: UrbanDictionarySlashCommand.CommandName, IsRemoved: true));
}
