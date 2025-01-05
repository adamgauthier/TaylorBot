using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;

public class CommandServerDisableSlashCommand(ICommandRepository commandRepository, IDisabledGuildCommandRepository disabledGuildCommandRepository) : ISlashCommand<CommandServerDisableSlashCommand.Options>
{
    public static string CommandName => "command server-disable";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString command);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var name = options.command.Value.Trim().ToLowerInvariant();
                var command = await commandRepository.FindCommandByAliasAsync(name);

                if (command == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"Could not find command '{options.command.Value}'."));
                }

                if (command.Name.StartsWith("command") || command.Name.StartsWith("owner"))
                {
                    return new EmbedResult(EmbedFactory.CreateError($"Sorry, '{command.Name}' can't be disabled because it's essential. 😕"));
                }

                if (command.Name.StartsWith("modmail"))
                {
                    return new EmbedResult(EmbedFactory.CreateError($"Please use **Discord's Server Settings > Apps > Integrations** to disable this command! 😕"));
                }

                await disabledGuildCommandRepository.DisableInAsync(guild, command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully disabled '{command.Name}' in this server ✅"));
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class CommandServerEnableSlashCommand(ICommandRepository commandRepository, IDisabledGuildCommandRepository disabledGuildCommandRepository) : ISlashCommand<CommandServerEnableSlashCommand.Options>
{
    public static string CommandName => "command server-enable";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedString command);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var name = options.command.Value.Trim().ToLowerInvariant();
                var command = await commandRepository.FindCommandByAliasAsync(name);

                if (command == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"Could not find command '{options.command.Value}'."));
                }

                await disabledGuildCommandRepository.EnableInAsync(guild, command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully enabled '{command.Name}' in this server ✅"));
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

