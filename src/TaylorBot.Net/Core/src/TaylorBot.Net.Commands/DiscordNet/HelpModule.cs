using Discord;
using Discord.Commands;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Commands.DiscordNet;

public static class SharedCommands
{
    public const string Help = "help";
}

[Name("Help")]
public class HelpModule(
    CommandService commands,
    IDisabledCommandRepository disabledCommandRepository,
    ICommandRepository commandRepository,
    ICommandRunner commandRunner
    ) : TaylorBotModule
{
    [Command(SharedCommands.Help)]
    [Summary("Lists help and information for a module's commands.")]
    public async Task<RuntimeResult> HelpAsync(
        [Summary("The module or command to list help for")]
        [Remainder]
        string? moduleOrCommand = null
    )
    {
        var command = new Command(DiscordNetContextMapper.MapToCommandMetadata(Context), async () =>
        {
            var module = moduleOrCommand == null ? commands.Modules.Single(m => m.Name == "Help") :
                commands.Modules.FirstOrDefault(m =>
                     m.Name.Replace("Module", "", StringComparison.InvariantCulture).Equals(moduleOrCommand, StringComparison.OrdinalIgnoreCase) ||
                     m.Commands.Any(c => c.Aliases.Select(a => a.ToUpperInvariant()).Contains(moduleOrCommand.ToUpperInvariant()))
                );

            if (module == null)
                return new EmptyResult();

            var builder = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

            if (module.Name != "Help")
            {
                builder
                    .WithTitle($"Module {module.Name}")
                    .WithDescription(BuildDescription(module));

                static string BuildDescription(ModuleInfo module)
                {
                    IEnumerable<string> descriptionLines = [module.Summary];

                    if (!string.IsNullOrEmpty(module.Remarks))
                        descriptionLines = descriptionLines.Append(module.Remarks);

                    if (module.Aliases.Any(a => !string.IsNullOrWhiteSpace(a)))
                        descriptionLines = descriptionLines.Append($"Prefix: `{module.Aliases[0]}`");

                    if (module.Submodules.Any())
                        descriptionLines = descriptionLines.Append($"Submodules:\n{string.Join("\n", module.Submodules.Select(m => $"- '{m.Name}' (`{m.Group}`)"))}");

                    return string.Join("\n", descriptionLines);
                }

                foreach (var command in module.Commands)
                {
                    await disabledCommandRepository.InsertOrGetCommandDisabledMessageAsync(new(command.Aliases[0]));

                    var alias = command.Aliases[0];
                    if (alias.Contains(' ', StringComparison.Ordinal))
                        alias = string.Join(' ', alias.Split(' ').Skip(1));

                    builder.AddField(field => field
                        .WithName($"**{alias}**")
                        .WithValue(string.Join("\n", new[] {
                            command.Summary,
                            !string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})" : "",
                            $"**Usage:** `{Context.GetUsage(command)}`"
                        }.Where(l => !string.IsNullOrWhiteSpace(l))))
                    );
                }
            }
            else
            {
                var commands = await commandRepository.GetAllCommandsAsync();
                string[] featuredModules = [
                    "Random 🎲",
                    "DiscordInfo 💬",
                    "Fun 🎭",
                    "Knowledge ❓",
                    "Media 📷",
                    "Points 💰",
                    "Reminders ⏰",
                    "Stats 📊",
                    "Weather 🌦",
                ];
                var groupedCommands = commands.Where(c => featuredModules.Contains(c.ModuleName)).GroupBy(c => c.ModuleName).OrderBy(g => g.Key);

                builder
                    .WithDescription($"Here are some command modules, use `{Context.CommandPrefix}help <command>` for more details. 😊")
                    .WithFields(groupedCommands.Select(g => new EmbedFieldBuilder()
                        .WithName(g.Key)
                        .WithValue(string.Join(", ", g.OrderBy(c => c.Name).Select(c => c.Name)))
                        .WithIsInline(true)
                    ));
            }

            return new EmbedResult(builder.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await commandRunner.RunSlashCommandAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
