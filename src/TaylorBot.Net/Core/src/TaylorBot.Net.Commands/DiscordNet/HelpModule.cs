using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Options;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.DiscordNet;

public class SharedCommands
{
    public const string Help = "help";
    public const string Diagnostic = "diagnostic";
}

[Name("Help")]
public class HelpModule : TaylorBotModule
{
    private readonly CommandService _commands;
    private readonly IDisabledCommandRepository _disabledCommandRepository;
    private readonly ICommandRepository _commandRepository;
    private readonly IOptionsMonitor<CommandApplicationOptions> _commandApplicationOptions;
    private readonly ICommandRunner _commandRunner;

    public HelpModule(
        CommandService commands,
        IDisabledCommandRepository disabledCommandRepository,
        ICommandRepository commandRepository,
        IOptionsMonitor<CommandApplicationOptions> commandApplicationOptions,
        ICommandRunner commandRunner
    )
    {
        _commands = commands;
        _disabledCommandRepository = disabledCommandRepository;
        _commandRepository = commandRepository;
        _commandApplicationOptions = commandApplicationOptions;
        _commandRunner = commandRunner;
    }

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
            var module = moduleOrCommand == null ? _commands.Modules.Single(m => m.Name == "Help") :
                _commands.Modules.FirstOrDefault(m =>
                     m.Name.Replace("Module", "").ToLowerInvariant() == moduleOrCommand.ToLowerInvariant() ||
                     m.Commands.Any(c => c.Aliases.Select(a => a.ToLowerInvariant()).Contains(moduleOrCommand.ToLowerInvariant()))
                );

            if (module == null)
                return new EmptyResult();

            var builder = new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithUserAsAuthor(Context.User);

            if (module.Name != "Help")
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
                        descriptionLines = descriptionLines.Append($"Submodules:\n{string.Join("\n", module.Submodules.Select(m => $"- '{m.Name}' (`{m.Group}`)"))}");

                    return string.Join("\n", descriptionLines);
                }

                foreach (var command in module.Commands)
                {
                    await _disabledCommandRepository.InsertOrGetCommandDisabledMessageAsync(new(command.Aliases[0]));

                    var alias = command.Aliases[0];
                    if (alias.Contains(' '))
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

            return new EmbedResult(builder.Build());
        });

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }

    [Command(SharedCommands.Diagnostic)]
    [Summary("Gets diagnostic information a TaylorBot component.")]
    public async Task<RuntimeResult> DiagnosticAsync(
        [Summary("The component to show diagnostic information for")]
        [Remainder]
        string? component = null
    )
    {
        var command = new Command(
            DiscordNetContextMapper.MapToCommandMetadata(Context),
            async () =>
            {
                if (component != _commandApplicationOptions.CurrentValue.ApplicationName)
                    return new EmptyResult();

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .WithUserAsAuthor(Context.CurrentUser)
                    .AddField("Guild Cache", (await Context.Client.GetGuildsAsync(CacheMode.CacheOnly)).Count, inline: true)
                    .AddField("DM Channels Cache", (await Context.Client.GetDMChannelsAsync(CacheMode.CacheOnly)).Count, inline: true);

                if (Context.Client is DiscordShardedClient shardedClient)
                {
                    embed.AddField("Shard Count", shardedClient.Shards.Count, inline: true);
                }

                if (Context.Client is BaseSocketClient socketClient)
                {
                    embed.AddField("Latency", $"{socketClient.Latency} ms", inline: true);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: new[] { new TaylorBotOwnerPrecondition() }
        );

        var context = DiscordNetContextMapper.MapToRunContext(Context);
        var result = await _commandRunner.RunAsync(command, context);

        return new TaylorBotResult(result, context);
    }
}
