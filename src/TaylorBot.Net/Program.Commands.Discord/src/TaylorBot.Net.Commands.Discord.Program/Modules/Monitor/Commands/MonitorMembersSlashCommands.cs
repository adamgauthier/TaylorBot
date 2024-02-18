using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorMembersSetSlashCommand(IPlusRepository plusRepository, IMemberLogChannelRepository memberLogChannelRepository) : ISlashCommand<MonitorMembersSetSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor members set");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;
                await memberLogChannelRepository.AddOrUpdateMemberLogAsync(channel);

                return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', [
                    $"Ok, I will now log member joins, leaves and bans in {channel.Mention}. 😊",
                    $"Use {context.MentionCommand("monitor members stop")} to stop monitoring member events."
                ])));
            },
            Preconditions: [
                new InGuildPrecondition(),
                new PlusPrecondition(plusRepository, PlusRequirement.PlusGuild),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class MonitorMembersShowSlashCommand(IMemberLogChannelRepository memberLogChannelRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor members show");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild!;
                var log = await memberLogChannelRepository.GetMemberLogForGuildAsync(guild);

                Embed? embed = null;

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', [
                            $"This server is configured to log member joins, leaves and bans in {channel.Mention}. ✅",
                            $"Use {context.MentionCommand("monitor members stop")} to stop monitoring member events in this server."
                        ]));
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(string.Join('\n', [
                            "I can't find the previously configured member events logging channel in this server. ❌",
                            $"Was it deleted? Use {context.MentionCommand("monitor members set")} to log member events in another channel."
                        ]));
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(string.Join('\n', [
                        "Member events monitoring is not configured in this server. ❌",
                        $"Use {context.MentionCommand("monitor members set")} to log member events in a specific channel."
                    ]));
                }

                return new EmbedResult(embed);
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class MonitorMembersStopSlashCommand(IMemberLogChannelRepository memberLogChannelRepository) : ISlashCommand<NoOptions>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("monitor members stop");

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                await memberLogChannelRepository.RemoveMemberLogAsync(context.Guild!);

                return new EmbedResult(EmbedFactory.CreateSuccess(string.Join('\n', [
                    "Ok, I will stop logging member events in this server. 😊",
                    $"Use {context.MentionCommand("monitor members set")} to log member events in a specific channel."
                ])));
            },
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}
