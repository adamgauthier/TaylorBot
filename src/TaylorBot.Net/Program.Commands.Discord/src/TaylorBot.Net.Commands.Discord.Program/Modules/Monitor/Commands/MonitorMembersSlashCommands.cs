using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Logs.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorMembersSetSlashCommand(IPlusRepository plusRepository, IMemberLogChannelRepository memberLogChannelRepository) : ISlashCommand<MonitorMembersSetSlashCommand.Options>
{
    public static string CommandName => "monitor members set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;

                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var log = await memberLogChannelRepository.GetMemberLogForGuildAsync(guild);
                if (log == null)
                {
                    return new EmbedResult(await AddOrUpdateAsync(context, channel));
                }
                else
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to change the member monitor channel to {channel.Mention}? ⚠️
                            Member joins, leaves and bans are currently being logged to {MentionUtils.MentionChannel(log.ChannelId)} 👈
                            """
                        )),
                        confirm: async () => new(await AddOrUpdateAsync(context, channel))
                    );
                }
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new PlusPrecondition(plusRepository, PlusRequirement.PlusGuild),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }

    private async ValueTask<Embed> AddOrUpdateAsync(RunContext context, GuildTextChannel channel)
    {
        await memberLogChannelRepository.AddOrUpdateMemberLogAsync(channel);

        return EmbedFactory.CreateSuccess(
            $"""
            Ok, I will now log member joins, leaves and bans in {channel.Mention} ✅
            Use {context.MentionCommand("monitor members stop")} to stop monitoring member events ↩️
            """);
    }
}

public class MonitorMembersShowSlashCommand(IMemberLogChannelRepository memberLogChannelRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "monitor members show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var log = await memberLogChannelRepository.GetMemberLogForGuildAsync(guild);

                Embed? embed = null;

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            This server is configured to log member joins, leaves and bans in {channel.Mention} ✅
                            Use {context.MentionCommand("monitor members stop")} to stop monitoring member events in this server ↩️
                            """);
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            I can't find the previously configured member events logging channel in this server ❌
                            Was it deleted? Use {context.MentionCommand("monitor members set")} to log member events in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(
                        $"""
                        Member events monitoring is not configured in this server ❌
                        Use {context.MentionCommand("monitor members set")} to log member events in a specific channel  ↩️
                        """);
                }

                return new EmbedResult(embed);
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class MonitorMembersStopSlashCommand(IMemberLogChannelRepository memberLogChannelRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "monitor members stop";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                await memberLogChannelRepository.RemoveMemberLogAsync(guild);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Ok, I will stop logging member events in this server ✅
                    Use {context.MentionCommand("monitor members set")} to log member events in a specific channel ↩️
                    """));
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}
