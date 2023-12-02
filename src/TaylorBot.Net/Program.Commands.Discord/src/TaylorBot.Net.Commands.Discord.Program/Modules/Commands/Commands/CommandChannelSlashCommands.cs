using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;

public class CommandChannelDisableSlashCommand : ISlashCommand<CommandChannelDisableSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("command channel-disable");

    public record Options(ParsedString command, ParsedTextChannelOrCurrent channel);

    private readonly ICommandRepository _commandRepository;
    private readonly IDisabledGuildChannelCommandRepository _disabledGuildChannelCommandRepository;

    public CommandChannelDisableSlashCommand(ICommandRepository commandRepository, IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository)
    {
        _commandRepository = commandRepository;
        _disabledGuildChannelCommandRepository = disabledGuildChannelCommandRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var name = options.command.Value.Trim().ToLowerInvariant();
                var command = await _commandRepository.FindCommandByAliasAsync(name);

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

                await _disabledGuildChannelCommandRepository.DisableInAsync(new(options.channel.Channel.Id.ToString()), guild, command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully disabled '{command.Name}' in {options.channel.Channel.Mention}. ✅"));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageChannels)
            }
        ));
    }
}

public class CommandChannelEnableSlashCommand : ISlashCommand<CommandChannelEnableSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("command channel-enable");

    public record Options(ParsedString command, ParsedTextChannelOrCurrent channel);

    private readonly ICommandRepository _commandRepository;
    private readonly IDisabledGuildChannelCommandRepository _disabledGuildChannelCommandRepository;

    public CommandChannelEnableSlashCommand(ICommandRepository commandRepository, IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository)
    {
        _commandRepository = commandRepository;
        _disabledGuildChannelCommandRepository = disabledGuildChannelCommandRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var name = options.command.Value.Trim().ToLowerInvariant();
                var command = await _commandRepository.FindCommandByAliasAsync(name);

                if (command == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"Could not find command '{options.command.Value}'."));
                }

                await _disabledGuildChannelCommandRepository.EnableInAsync(new(options.channel.Channel.Id.ToString()), guild, command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully enabled '{command.Name}' in {options.channel.Channel.Mention}. ✅"));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageChannels)
            }
        ));
    }
}

