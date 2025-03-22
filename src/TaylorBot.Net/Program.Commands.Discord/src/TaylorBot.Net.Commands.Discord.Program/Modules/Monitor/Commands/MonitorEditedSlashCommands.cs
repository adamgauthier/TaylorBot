﻿using Discord;
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

public class MonitorEditedSetSlashCommand(
    IEditedLogChannelRepository editedLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    PlusPrecondition.Factory plusPrecondition,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<MonitorEditedSetSlashCommand.Options>
{
    public static string CommandName => "monitor edited set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        inGuild.Create(botMustBeInGuild: true),
        plusPrecondition.Create(PlusRequirement.PlusGuild),
        userHasPermission.Create(GuildPermission.ManageGuild)
    ];

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

                var log = await editedLogChannelRepository.GetEditedLogForGuildAsync(guild);
                if (log == null)
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            """
                            You are configuring edited message monitoring for this server. In doing so, you understand that:
                            - TaylorBot will **save the content of all messages sent in the server for 10 minutes** to provide this feature.
                            - Edited messages that are older than this time window will be logged but the content before edit won't be available.
                            - Bots often use message editing to provide features. These **will not** be logged as they could quickly clutter your logs.
                            """)),
                        InteractionCustomId.Create(MonitorEditedSetConfirmButtonHandler.CustomIdName, [new("channel", $"{channel.Id}")])
                    );
                }
                else
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to change the edited message monitor channel to {channel.Mention}? ⚠️
                            Edited messages are currently being logged to {MentionUtils.MentionChannel(log.ChannelId)} 👈
                            """
                        )),
                        InteractionCustomId.Create(MonitorEditedSetConfirmButtonHandler.CustomIdName, [new("channel", $"{channel.Id}")])
                    );
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async ValueTask<Embed> AddOrUpdateAsync(RunContext context, GuildTextChannel channel)
    {
        await editedLogChannelRepository.AddOrUpdateEditedLogAsync(channel);

        return EmbedFactory.CreateSuccess(
            $"""
            Ok, I will now log edited messages in {channel.Mention}. **Please wait up to 5 minutes for changes to take effect** ⌚
            Use {mention.SlashCommand("monitor edited stop", context)} to stop monitoring edited messages ↩️
            """);
    }
}

public class MonitorEditedSetConfirmButtonHandler(InteractionResponseClient responseClient, MonitorEditedSetSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.MonitorEditedSetConfirm;

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

public class MonitorEditedShowSlashCommand(
    IEditedLogChannelRepository editedLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "monitor edited show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var log = await editedLogChannelRepository.GetEditedLogForGuildAsync(guild);

                Embed? embed = null;

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            This server is configured to log edited messages in {channel.Mention} ✅
                            Use {mention.SlashCommand("monitor edited stop", context)} to stop monitoring edited messages in this server ↩️
                            """);
                    }
                    else
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            I can't find the previously configured edited messages logging channel in this server ❌
                            Was it deleted? Use {mention.SlashCommand("monitor edited set", context)} to log edited messages in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(
                        $"""
                        Edited message monitoring is not configured in this server ❌
                        Use {mention.SlashCommand("monitor edited set", context)} to log edited messages in a specific channel ↩️
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

public class MonitorEditedStopSlashCommand(
    IEditedLogChannelRepository editedLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<NoOptions>
{
    public static string CommandName => "monitor edited stop";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                await editedLogChannelRepository.RemoveEditedLogAsync(guild);

                return new EmbedResult(EmbedFactory.CreateSuccess(
                    $"""
                    Ok, I will stop logging edited messages in this server. **Please wait up to 5 minutes for changes to take effect** ⌚
                    Use {mention.SlashCommand("monitor edited set", context)} to log edited messages in a specific channel ↩️
                    """));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}
