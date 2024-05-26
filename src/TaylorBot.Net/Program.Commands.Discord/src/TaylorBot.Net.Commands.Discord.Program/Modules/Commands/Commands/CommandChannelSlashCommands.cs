using Discord;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands;

public class CommandChannelDisableSlashCommand(ICommandRepository commandRepository, IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository) : ISlashCommand<CommandChannelDisableSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("command channel-disable");

    public record Options(ParsedString command, ParsedTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
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

                await disabledGuildChannelCommandRepository.DisableInAsync(options.channel.Channel, command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully disabled '{command.Name}' in {options.channel.Channel.Mention}. ✅"));
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageChannels)
            ]
        ));
    }
}

public class CommandChannelEnableSlashCommand(ICommandRepository commandRepository, IDisabledGuildChannelCommandRepository disabledGuildChannelCommandRepository) : ISlashCommand<CommandChannelEnableSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("command channel-enable");

    public record Options(ParsedString command, ParsedTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var name = options.command.Value.Trim().ToLowerInvariant();
                var command = await commandRepository.FindCommandByAliasAsync(name);

                if (command == null)
                {
                    return new EmbedResult(EmbedFactory.CreateError($"Could not find command '{options.command.Value}'."));
                }

                await disabledGuildChannelCommandRepository.EnableInAsync(options.channel.Channel, command.Name);

                return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully enabled '{command.Name}' in {options.channel.Channel.Mention}. ✅"));
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageChannels)
            ]
        ));
    }
}

