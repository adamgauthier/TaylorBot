using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Preconditions;

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

        public HelpModule(CommandService commands, IDisabledCommandRepository disabledCommandRepository)
        {
            _commands = commands;
            _disabledCommandRepository = disabledCommandRepository;
        }

        [Command("help")]
        [Alias("command")]
        [Summary("Lists help and information for a module's commands.")]
        public async Task<RuntimeResult> HelpAsync(
            [Summary("The module or command to list help for")]
            [Remainder]
            string moduleOrCommand = null
        )
        {
            if (moduleOrCommand != null)
            {
                var module = _commands.Modules.FirstOrDefault(m =>
                    m.Name.Replace("Module", "").ToLowerInvariant() == moduleOrCommand.ToLowerInvariant() ||
                    m.Commands.Any(c => c.Aliases.Select(a => a.ToLowerInvariant()).Contains(moduleOrCommand.ToLowerInvariant()))
                );

                if (module != default && module.Name != ModuleNames.Help)
                {
                    var builder = new EmbedBuilder()
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

                        builder.AddField(field => field
                            .WithName($"**{command.Name}**")
                            .WithValue(string.Join("\n", new[] {
                                command.Summary,
                                (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})" : ""),
                                $"**Usage:** `{Context.GetUsage(command)}`"
                            }.Where(l => !string.IsNullOrWhiteSpace(l))))
                        );
                    }

                    return new TaylorBotEmbedResult(builder.Build());
                }
            }

            return new TaylorBotEmptyResult();
        }
    }
}
