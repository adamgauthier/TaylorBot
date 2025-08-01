﻿using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

public class ModLogSetSlashCommand(
    IModLogChannelRepository modLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<ModLogSetSlashCommand.Options>
{
    public static string CommandName => "mod log set";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        inGuild.Create(botMustBeInGuild: true),
        userHasPermission.Create(GuildPermission.ManageGuild)
    ];

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var modLog = await modLogChannelRepository.GetModLogForGuildAsync(guild);
                if (modLog == null)
                {
                    return new EmbedResult(await AddOrUpdateAsync(context, options.channel.Channel));
                }
                else
                {
                    return MessageResult.CreatePrompt(
                        new(EmbedFactory.CreateWarning(
                            $"""
                            Are you sure you want to change the mod log channel to {options.channel.Channel.Mention}? ⚠️
                            Moderation command usage is currently being logged to {MentionUtils.MentionChannel(modLog.ChannelId)} 👈
                            """
                        )),
                        InteractionCustomId.Create(ModLogSetConfirmButtonHandler.CustomIdName, [new("channel", $"{options.channel.Channel.Id}")])
                    );
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async ValueTask<Embed> AddOrUpdateAsync(RunContext context, GuildTextChannel channel)
    {
        await modLogChannelRepository.AddOrUpdateModLogAsync(channel);

        return new EmbedBuilder()
            .WithColor(TaylorBotColors.SuccessColor)
            .WithDescription(
                $"""
                Ok, I will now log moderation command usage in {channel.Mention} ✅
                Use {mention.SlashCommand("mod log show", context)} to see the current configuration ↩️
                """)
        .Build();
    }
}

public class ModLogSetConfirmButtonHandler(IInteractionResponseClient responseClient, ModLogSetSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModLogSetConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        SnowflakeId channelId = button.CustomId.ParsedData["channel"];
        var channel = await guild.GetChannelAsync(channelId);

        if (channel == null)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                $"The selected channel {MentionUtils.MentionChannel(channelId)} no longer exists 🤔"));
            return;
        }

        GuildTextChannel textChannel = new(channelId, guild.Id, channel.ChannelType);
        var embed = await command.AddOrUpdateAsync(context, textChannel);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(embed));
    }
}

public class ModLogShowSlashCommand(
    IModLogChannelRepository modLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "mod log show";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var modLog = await modLogChannelRepository.GetModLogForGuildAsync(guild);

                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);
                List<InteractionComponent> components = [];

                if (modLog != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(modLog.ChannelId.Id);
                    if (channel != null)
                    {
                        embed.WithDescription(
                            $"""
                            This server is configured to log moderation command usage in {channel.Mention} ✅
                            """);
                        components.Add(InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
                            style: InteractionButtonStyle.Danger,
                            custom_id: InteractionCustomId.Create(ModLogStopButtonHandler.CustomIdName).RawId,
                            label: "Stop Logging",
                            emoji: new("🗑"))));
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            I can't find the previously configured moderation command usage logging channel in this server ❌
                            Was it deleted? Use {mention.SlashCommand("mod log set", context)} to log moderation command usage in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        There is no moderation command usage logging configured in this server ❌
                        Use {mention.SlashCommand("mod log set", context)} to log moderation command usage in a specific channel ↩️
                        """);
                }

                return new MessageResult(new(new(embed.Build()), components));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild),
            ]
        ));
    }
}

public class ModLogStopButtonHandler(
    IModLogChannelRepository modLogChannelRepository,
    IInteractionResponseClient responseClient,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModLogStop;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: [
            inGuild.Create(),
            userHasPermission.Create(GuildPermission.ManageGuild),
        ],
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var guild = context.Guild;
        ArgumentNullException.ThrowIfNull(guild);

        await modLogChannelRepository.RemoveModLogAsync(guild);

        var embed = EmbedFactory.CreateSuccessEmbed(
            $"""
            Moderation command usage logging is now disabled in this server ✅
            Use {mention.SlashCommand("mod log set", context)} to re-enable it ↩️
            """);

        await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
    }
}
