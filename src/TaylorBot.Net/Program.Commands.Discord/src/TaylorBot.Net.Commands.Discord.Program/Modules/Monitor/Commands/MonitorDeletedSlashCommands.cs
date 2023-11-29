using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorDeletedSetSlashCommand : ISlashCommand<MonitorDeletedSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor deleted set");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    private readonly IPlusRepository _plusRepository;
    private readonly IDeletedLogChannelRepository _deletedLogChannelRepository;

    public MonitorDeletedSetSlashCommand(IPlusRepository plusRepository, IDeletedLogChannelRepository deletedLogChannelRepository)
    {
        _plusRepository = plusRepository;
        _deletedLogChannelRepository = deletedLogChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                return new(MessageResult.CreatePrompt(
                    new(EmbedFactory.CreateWarning(string.Join('\n', new[] {
                        $"You are configuring deleted message monitoring for this server. In doing so, you understand that:",
                        $"- TaylorBot will **save the content of all messages sent in the server for 10 minutes** to provide this feature.",
                        $"- Deleted messages that are older than this time window will be logged but the message content won't be available.",
                    }))),
                    confirm: async () =>
                    {
                        var channel = options.channel.Channel;
                        await _deletedLogChannelRepository.AddOrUpdateDeletedLogAsync(channel);

                        return new MessageContent(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            $"Ok, I will now log deleted messages in {channel.Mention}. **Please wait up to 5 minutes for changes to take effect.** ⌚",
                            $"Use {context.MentionCommand("monitor deleted stop")} to stop monitoring deleted messages."
                        })));
                    }
                ));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new PlusPrecondition(_plusRepository, PlusRequirement.PlusGuild),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}

public class MonitorDeletedShowSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor deleted show");

    private readonly IDeletedLogChannelRepository _deletedLogChannelRepository;

    public MonitorDeletedShowSlashCommand(IDeletedLogChannelRepository deletedLogChannelRepository)
    {
        _deletedLogChannelRepository = deletedLogChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var log = await _deletedLogChannelRepository.GetDeletedLogForGuildAsync(guild);

                Embed? embed = null;

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            $"This server is configured to log deleted messages in {channel.Mention}. ✅",
                            $"Use {context.MentionCommand("monitor deleted stop")} to stop monitoring deleted messages in this server."
                        }));
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            "I can't find the previously configured deleted messages logging channel in this server. ❌",
                            $"Was it deleted? Use {context.MentionCommand("monitor deleted set")} to log deleted messages in another channel."
                        }));
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                        "Deleted message monitoring is not configured in this server. ❌",
                        $"Use {context.MentionCommand("monitor deleted set")} to log deleted messages in a specific channel."
                    }));
                }

                return new EmbedResult(embed);
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}

public class MonitorDeletedStopSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor deleted stop");

    private readonly IDeletedLogChannelRepository _deletedLogChannelRepository;

    public MonitorDeletedStopSlashCommand(IDeletedLogChannelRepository deletedLogChannelRepository)
    {
        _deletedLogChannelRepository = deletedLogChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _deletedLogChannelRepository.RemoveDeletedLogAsync(context.Guild!);

                return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                    "Ok, I will stop logging deleted messages in this server. **Please wait up to 5 minutes for changes to take effect.** ⌚",
                    $"Use {context.MentionCommand("monitor deleted set")} to log deleted messages in a specific channel."
                })));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}
