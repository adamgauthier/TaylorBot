using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorMembersSetSlashCommand : ISlashCommand<MonitorMembersSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor members set");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    private readonly IPlusRepository _plusRepository;
    private readonly IMemberLogChannelRepository _memberLogChannelRepository;

    public MonitorMembersSetSlashCommand(IPlusRepository plusRepository, IMemberLogChannelRepository memberLogChannelRepository)
    {
        _plusRepository = plusRepository;
        _memberLogChannelRepository = memberLogChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;
                await _memberLogChannelRepository.AddOrUpdateMemberLogAsync(channel);

                return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                    $"Ok, I will now log member joins, leaves and bans in {channel.Mention}. 😊",
                    $"Use {context.MentionCommand("monitor members stop")} to stop monitoring member events."
                })));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new PlusPrecondition(_plusRepository, PlusRequirement.PlusGuild),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}

public class MonitorMembersShowSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor members show");

    private readonly IMemberLogChannelRepository _memberLogChannelRepository;

    public MonitorMembersShowSlashCommand(IMemberLogChannelRepository memberLogChannelRepository)
    {
        _memberLogChannelRepository = memberLogChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var log = await _memberLogChannelRepository.GetMemberLogForGuildAsync(guild);

                Embed? embed = null;

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            $"This server is configured to log member joins, leaves and bans in {channel.Mention}. ✅",
                            $"Use {context.MentionCommand("monitor members stop")} to stop monitoring member events in this server."
                        }));
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                            "I can't find the previously configured member events logging channel in this server. ❌",
                            $"Was it deleted? Use {context.MentionCommand("monitor members set")} to log member events in another channel."
                        }));
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                        "Member events monitoring is not configured in this server. ❌",
                        $"Use {context.MentionCommand("monitor members set")} to log member events in a specific channel."
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

public class MonitorMembersStopSlashCommand : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor members stop");

    private readonly IMemberLogChannelRepository _memberLogChannelRepository;

    public MonitorMembersStopSlashCommand(IMemberLogChannelRepository memberLogChannelRepository)
    {
        _memberLogChannelRepository = memberLogChannelRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await _memberLogChannelRepository.RemoveMemberLogAsync(context.Guild!);

                return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', new[] {
                    "Ok, I will stop logging member events in this server. 😊",
                    $"Use {context.MentionCommand("monitor members set")} to log member events in a specific channel."
                })));
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            }
        ));
    }
}
