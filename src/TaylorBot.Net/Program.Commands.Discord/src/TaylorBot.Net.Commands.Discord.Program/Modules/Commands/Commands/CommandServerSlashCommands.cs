using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Commands.Commands
{
    public class CommandServerDisableSlashCommand : ISlashCommand<CommandServerDisableSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("command server-disable");

        public record Options(ParsedString command);

        private readonly ICommandRepository _commandRepository;
        private readonly IDisabledGuildCommandRepository _disabledGuildCommandRepository;

        public CommandServerDisableSlashCommand(ICommandRepository commandRepository, IDisabledGuildCommandRepository disabledGuildCommandRepository)
        {
            _commandRepository = commandRepository;
            _disabledGuildCommandRepository = disabledGuildCommandRepository;
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

                    await _disabledGuildCommandRepository.DisableInAsync(guild, command.Name);

                    return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully disabled '{command.Name}' in '{guild.Name}'. ✅"));
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                }
            ));
        }
    }

    public class CommandServerEnableSlashCommand : ISlashCommand<CommandServerEnableSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("command server-enable");

        public record Options(ParsedString command);

        private readonly ICommandRepository _commandRepository;
        private readonly IDisabledGuildCommandRepository _disabledGuildCommandRepository;

        public CommandServerEnableSlashCommand(ICommandRepository commandRepository, IDisabledGuildCommandRepository disabledGuildCommandRepository)
        {
            _commandRepository = commandRepository;
            _disabledGuildCommandRepository = disabledGuildCommandRepository;
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

                    await _disabledGuildCommandRepository.EnableInAsync(guild, command.Name);

                    return new EmbedResult(EmbedFactory.CreateSuccess($"Successfully enabled '{command.Name}' in '{guild.Name}'. ✅"));
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                }
            ));
        }
    }
}

