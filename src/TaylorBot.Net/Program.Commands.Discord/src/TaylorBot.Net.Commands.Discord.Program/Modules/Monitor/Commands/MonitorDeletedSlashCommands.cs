using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorDeletedSetSlashCommand(IPlusRepository plusRepository, IDeletedLogChannelRepository deletedLogChannelRepository) : ISlashCommand<MonitorDeletedSetSlashCommand.Options>
{
    public static string CommandName => "monitor deleted set";

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

                var log = await deletedLogChannelRepository.GetDeletedLogForGuildAsync(guild);
                if (log == null)
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            """
                            You are configuring deleted message monitoring for this server. In doing so, you understand that:
                            - TaylorBot will **save the content of all messages sent in the server for 10 minutes** to provide this feature.
                            - Deleted messages that are older than this time window will be logged but the message content won't be available.
                            """)),
                        confirm: async () => new(await AddOrUpdateAsync(context, channel))
                    );
                }
                else
                {

                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to change the deleted message monitor channel to {channel.Mention}? ⚠️
                            Deleted messages are currently being logged to {MentionUtils.MentionChannel(log.ChannelId)} 👈
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
        await deletedLogChannelRepository.AddOrUpdateDeletedLogAsync(channel);

        return EmbedFactory.CreateSuccess(
            $"""
            Ok, I will now log deleted messages in {channel.Mention}. **Please wait up to 5 minutes for changes to take effect** ⌚
            Use {context.MentionCommand("monitor deleted stop")} to stop monitoring deleted messages ↩️
            """);
    }
}

public class MonitorDeletedShowSlashCommand(IDeletedLogChannelRepository deletedLogChannelRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "monitor deleted show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var log = await deletedLogChannelRepository.GetDeletedLogForGuildAsync(guild);

                Embed? embed = null;

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            This server is configured to log deleted messages in {channel.Mention} ✅
                            Use {context.MentionCommand("monitor deleted stop")} to stop monitoring deleted messages in this server ↩️
                            """);
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            I can't find the previously configured deleted messages logging channel in this server ❌
                            Was it deleted? Use {context.MentionCommand("monitor deleted set")} to log deleted messages in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(
                        $"""
                        Deleted message monitoring is not configured in this server ❌
                        Use {context.MentionCommand("monitor deleted set")} to log deleted messages in a specific channel ↩️
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

public class MonitorDeletedStopSlashCommand(IDeletedLogChannelRepository deletedLogChannelRepository) : ISlashCommand<NoOptions>
{
    public static string CommandName => "monitor deleted stop";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                await deletedLogChannelRepository.RemoveDeletedLogAsync(guild);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Ok, I will stop logging deleted messages in this server. **Please wait up to 5 minutes for changes to take effect** ⌚
                    Use {context.MentionCommand("monitor deleted set")} to log deleted messages in a specific channel ↩️
                    """));
            },
            Preconditions: [
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.ManageGuild)
            ]
        ));
    }
}
