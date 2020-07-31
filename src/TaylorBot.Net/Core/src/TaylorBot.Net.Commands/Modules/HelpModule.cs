using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Modules
{
    public class ModuleNames
    {
        public const string Help = "Help";
    }

    [Name(ModuleNames.Help)]
    public class HelpModule : TaylorBotModule
    {
        private readonly CommandService _commands;
        private readonly IDisabledCommandRepository _disabledCommandRepository;
        private readonly ICommandRepository _commandRepository;

        public HelpModule(CommandService commands, IDisabledCommandRepository disabledCommandRepository, ICommandRepository commandRepository)
        {
            _commands = commands;
            _disabledCommandRepository = disabledCommandRepository;
            _commandRepository = commandRepository;
        }

        [Command("help")]
        [Alias("command")]
        [Summary("Lists help and information for a module's commands.")]
        public async Task<RuntimeResult> HelpAsync(
            [Summary("The module or command to list help for")]
            [Remainder]
            string? moduleOrCommand = null
        )
        {
            var module = moduleOrCommand == null ? _commands.Modules.Single(m => m.Name == ModuleNames.Help) :
                _commands.Modules.FirstOrDefault(m =>
                     m.Name.Replace("Module", "").ToLowerInvariant() == moduleOrCommand.ToLowerInvariant() ||
                     m.Commands.Any(c => c.Aliases.Select(a => a.ToLowerInvariant()).Contains(moduleOrCommand.ToLowerInvariant()))
                );

            if (module == null)
                return new TaylorBotEmptyResult();

            var builder = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(Context.User);

            if (module.Name != ModuleNames.Help)
            {
                builder
                    .WithTitle($"Module {module.Name}")
                    .WithDescription(BuildDescription(module));

                static string BuildDescription(ModuleInfo module)
                {
                    IEnumerable<string> descriptionLines = new[] { module.Summary };

                    if (!string.IsNullOrEmpty(module.Remarks))
                        descriptionLines = descriptionLines.Append(module.Remarks);

                    if (module.Aliases.Any(a => !string.IsNullOrWhiteSpace(a)))
                        descriptionLines = descriptionLines.Append($"Prefix: `{module.Aliases.First()}`");

                    if (module.Submodules.Any())
                        descriptionLines = descriptionLines.Append($"Submodules: {module.Submodules.Select(m => m.Name)}");

                    return string.Join("\n", descriptionLines);
                }

                foreach (var command in module.Commands)
                {
                    await _disabledCommandRepository.InsertOrGetIsCommandDisabledAsync(command);

                    var alias = command.Aliases.First();
                    if (alias.Contains(' '))
                        alias = string.Join(' ', alias.Split(' ').Skip(1));

                    builder.AddField(field => field
                        .WithName($"**{alias}**")
                        .WithValue(string.Join("\n", new[] {
                            command.Summary,
                            (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})" : ""),
                            $"**Usage:** `{Context.GetUsage(command)}`"
                        }.Where(l => !string.IsNullOrWhiteSpace(l))))
                    );
                }
            }
            else
            {
                var commands = await _commandRepository.GetAllCommandsAsync();
                var featuredModules = new[] {
                    "Random 🎲",
                    "DiscordInfo 💬",
                    "Fun 🎭",
                    "Knowledge ❓",
                    "Media 📷",
                    "Points 💰",
                    "Reminders ⏰",
                    "Stats 📊",
                    "Weather 🌦"
                };
                var groupedCommands = commands.Where(c => featuredModules.Contains(c.ModuleName)).GroupBy(c => c.ModuleName).OrderBy(g => g.Key);

                builder
                    .WithDescription($"Here are some command modules, use `{Context.CommandPrefix}help <command>` for more details. 😊")
                    .WithFields(groupedCommands.Select(g => new EmbedFieldBuilder()
                        .WithName(g.Key)
                        .WithValue(string.Join(", ", g.OrderBy(c => c.Name).Select(c => c.Name)))
                        .WithIsInline(true)
                    ));
            }

            return new TaylorBotEmbedResult(builder.Build());
        }
    }
}
