using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Monitor.Commands;

public class MonitorDeletedSetSlashCommand(
    IDeletedLogChannelRepository deletedLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    PlusPrecondition.Factory plusPrecondition,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<MonitorDeletedSetSlashCommand.Options>
{
    public static string CommandName => "monitor deleted set";

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
                        InteractionCustomId.Create(MonitorDeletedSetConfirmButtonHandler.CustomIdName, [new("channel", $"{channel.Id}")])
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
                        InteractionCustomId.Create(MonitorDeletedSetConfirmButtonHandler.CustomIdName, [new("channel", $"{channel.Id}")])
                    );
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async ValueTask<Embed> AddOrUpdateAsync(RunContext context, GuildTextChannel channel)
    {
        await deletedLogChannelRepository.AddOrUpdateDeletedLogAsync(channel);

        return EmbedFactory.CreateSuccess(
            $"""
            Ok, I will now log deleted messages in {channel.Mention}. **Please wait up to 5 minutes for changes to take effect** ⌚
            Use {mention.SlashCommand("monitor deleted show", context)} to see the current configuration ↩️
            """);
    }
}

public class MonitorDeletedSetConfirmButtonHandler(IInteractionResponseClient responseClient, MonitorDeletedSetSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.MonitorDeletedSetConfirm;

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

public class MonitorDeletedShowSlashCommand(
    IDeletedLogChannelRepository deletedLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : ISlashCommand<NoOptions>
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

                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);
                List<InteractionComponent> components = [];

                if (log != null)
                {
                    var channel = (ITextChannel?)await guild.GetChannelAsync(log.ChannelId.Id);
                    if (channel != null)
                    {
                        embed.WithDescription(
                            $"""
                            This server is configured to log deleted messages in {channel.Mention} ✅
                            """);
                        components.Add(InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
                            style: InteractionButtonStyle.Danger,
                            custom_id: InteractionCustomId.Create(MonitorDeletedStopButtonHandler.CustomIdName).RawId,
                            label: "Stop Monitoring",
                            emoji: new("🗑"))));
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            I can't find the previously configured deleted messages logging channel in this server ❌
                            Was it deleted? Use {mention.SlashCommand("monitor deleted set", context)} to log deleted messages in another channel ↩️
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        Deleted message monitoring is not configured in this server ❌
                        Use {mention.SlashCommand("monitor deleted set", context)} to log deleted messages in a specific channel ↩️
                        """);
                }

                return new MessageResult(new(new(embed.Build()), components));
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.ManageGuild)
            ]
        ));
    }
}

public class MonitorDeletedStopButtonHandler(
    IDeletedLogChannelRepository deletedLogChannelRepository,
    IInteractionResponseClient responseClient,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild,
    CommandMentioner mention) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.MonitorDeletedStop;

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

        await deletedLogChannelRepository.RemoveDeletedLogAsync(guild);

        var embed = EmbedFactory.CreateSuccessEmbed(
            $"""
            Deleted message logging is now disabled in this server. **Please wait up to 5 minutes for changes to take effect** ⌚
            Use {mention.SlashCommand("monitor deleted set", context)} to re-enable it ↩️
            """);

        await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
    }
}
