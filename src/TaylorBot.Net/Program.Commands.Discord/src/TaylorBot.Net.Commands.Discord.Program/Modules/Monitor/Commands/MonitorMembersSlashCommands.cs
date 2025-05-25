using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorMembersSetSlashCommand(
    IMemberLogChannelRepository memberLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    PlusPrecondition.Factory plusPrecondition,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<MonitorMembersSetSlashCommand.Options>
{
    public static string CommandName => "monitor members set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        inGuild.Create(botMustBeInGuild: true),
        plusPrecondition.Create(PlusRequirement.PlusGuild),
        userHasPermission.Create(GuildPermission.ManageGuild)
    ];

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
                        InteractionCustomId.Create(MonitorMembersSetConfirmButtonHandler.CustomIdName, [new("channel", $"{channel.Id}")])
                    );
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async ValueTask<Embed> AddOrUpdateAsync(RunContext context, GuildTextChannel channel)
    {
        await memberLogChannelRepository.AddOrUpdateMemberLogAsync(channel);

        return EmbedFactory.CreateSuccess(
            $"""
            Ok, I will now log member joins, leaves and bans in {channel.Mention} ✅
            Use {mention.SlashCommand("monitor members stop", context)} to stop monitoring member events ↩️
            """);
    }
}

public class MonitorMembersSetConfirmButtonHandler(IInteractionResponseClient responseClient, MonitorMembersSetSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.MonitorMembersSetConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        SnowflakeId channelId = button.CustomId.ParsedData["channel"];
        var discordChannel = await guild.GetChannelAsync(channelId);

        if (discordChannel == null)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                $"The selected channel {MentionUtils.MentionChannel(channelId)} no longer exists 🤔"));
            return;
        }

        GuildTextChannel channel = new(channelId, guild.Id, discordChannel.ChannelType);
        var embed = await command.AddOrUpdateAsync(context, channel);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(embed));
    }
}

public class MonitorMembersShowSlashCommand(
    IMemberLogChannelRepository memberLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<NoOptions>
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
                            Use {mention.SlashCommand("monitor members stop", context)} to stop monitoring member events in this server ↩️
                            """);
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            I can't find the previously configured member events logging channel in this server ❌
                            Was it deleted? Use {mention.SlashCommand("monitor members set", context)} to log member events in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(
                        $"""
                        Member events monitoring is not configured in this server ❌
                        Use {mention.SlashCommand("monitor members set", context)} to log member events in a specific channel  ↩️
                        """);
                }

                return new EmbedResult(embed);
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class MonitorMembersStopSlashCommand(
    IMemberLogChannelRepository memberLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<NoOptions>
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
                    Use {mention.SlashCommand("monitor members set", context)} to log member events in a specific channel ↩️
                    """));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}
