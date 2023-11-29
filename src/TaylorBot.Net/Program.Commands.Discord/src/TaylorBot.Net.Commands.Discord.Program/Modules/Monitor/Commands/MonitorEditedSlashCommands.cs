using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands
{
    public class MonitorEditedSetSlashCommand : ISlashCommand<MonitorEditedSetSlashCommand.Options>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("monitor edited set");

        public record Options(ParsedNonThreadTextChannelOrCurrent channel);

        private readonly IPlusRepository _plusRepository;
        private readonly IEditedLogChannelRepository _editedLogChannelRepository;

        public MonitorEditedSetSlashCommand(IPlusRepository plusRepository, IEditedLogChannelRepository editedLogChannelRepository)
        {
            _plusRepository = plusRepository;
            _editedLogChannelRepository = editedLogChannelRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                () =>
                {
                    return new(MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(string.Join('\n', new[] {
                            $"You are configuring edited message monitoring for this server. In doing so, you understand that:",
                            $"- TaylorBot will **save the content of all messages sent in the server for 10 minutes** to provide this feature.",
                            $"- Edited messages that are older than this time window will be logged but the content before edit won't be available.",
                            $"- Bots often use message editing to provide features. These **will not** be logged as they could quickly clutter your logs.",
                        }))),
                        confirm: async () =>
                        {
                            var channel = options.channel.Channel;
                            await _editedLogChannelRepository.AddOrUpdateEditedLogAsync(channel);

                            return new MessageContent(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                                $"Ok, I will now log edited messages in {channel.Mention}. **Please wait up to 5 minutes for changes to take effect.** ⌚",
                                $"Use {context.MentionCommand("monitor edited stop")} to stop monitoring edited messages."
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

    public class MonitorEditedShowSlashCommand : ISlashCommand<NoOptions>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("monitor edited show");

        private readonly IEditedLogChannelRepository _editedLogChannelRepository;

        public MonitorEditedShowSlashCommand(IEditedLogChannelRepository editedLogChannelRepository)
        {
            _editedLogChannelRepository = editedLogChannelRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var guild = context.Guild!;
                    var log = await _editedLogChannelRepository.GetEditedLogForGuildAsync(guild);

                    Embed? embed = null;

                    if (log != null)
                    {
                        var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                        if (channel != null)
                        {
                            embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                                $"This server is configured to log edited messages in {channel.Mention}. ✅",
                                $"Use {context.MentionCommand("monitor edited stop")} to stop monitoring edited messages in this server."
                            }));
                        }
                        else
                        {
                            embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                                "I can't find the previously configured edited messages logging channel in this server. ❌",
                                $"Was it deleted? Use {context.MentionCommand("monitor edited set")} to log edited messages in another channel."
                            }));
                        }
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            "Edited message monitoring is not configured in this server. ❌",
                            $"Use {context.MentionCommand("monitor edited set")} to log edited messages in a specific channel."
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

    public class MonitorEditedStopSlashCommand : ISlashCommand<NoOptions>
    {
        public ISlashCommandInfo Info => new MessageCommandInfo("monitor edited stop");

        private readonly IEditedLogChannelRepository _editedLogChannelRepository;

        public MonitorEditedStopSlashCommand(IEditedLogChannelRepository editedLogChannelRepository)
        {
            _editedLogChannelRepository = editedLogChannelRepository;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    await _editedLogChannelRepository.RemoveEditedLogAsync(context.Guild!);

                    return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                        "Ok, I will stop logging edited messages in this server. **Please wait up to 5 minutes for changes to take effect.** ⌚",
                        $"Use {context.MentionCommand("monitor edited set")} to log edited messages in a specific channel."
                    })));
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
                }
            ));
        }
    }
}
