using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using static TaylorBot.Net.Commands.PostExecution.InteractionResponse;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailConfigSlashCommand(
    IModMailLogChannelRepository modMailLogChannelRepository,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    CommandMentioner mention,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "modmail config";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        inGuild.Create(botMustBeInGuild: true),
        userHasPermission.Create(GuildPermission.ManageGuild)
    ];

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild;
                ArgumentNullException.ThrowIfNull(guild);
                ArgumentNullException.ThrowIfNull(guild.Fetched);

                var modMailLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);

                Embed embed;

                if (modMailLog != null)
                {
                    var channel = (ITextChannel?)await guild.Fetched.GetChannelAsync(modMailLog.ChannelId.Id);
                    if (channel != null)
                    {
                        embed = EmbedFactory.CreateSuccess(
                            $"""
                            ## Mod Mail Configuration ⚙️
                            Mod Mail is currently enabled in this server in {channel.Mention} ✅
                            You can change the channel or disable below ⬇️
                            """);
                    }
                    else
                    {
                        embed = EmbedFactory.CreateWarning(
                            $"""
                            ## Mod Mail Configuration ⚙️
                            I can't find the configured Mod Mail channel in this server ⚠️
                            Was it deleted? Does TaylorBot have access? 🛠️
                            You can change it to another channel below ⬇️
                            """);
                    }
                }
                else
                {
                    embed = EmbedFactory.CreateSuccess(
                        $"""
                        ## What is Mod Mail? ✉️
                        Mod Mail is a feature that allows users to send messages to moderators.
                        - Users send mail with {mention.SlashCommand("modmail message-mods", context)} 📨
                        - TaylorBot will send the messages to a channel in this server 📬
                        - Moderators can respond to these messages anonymously 💌

                        Enable Mod Mail in your server by picking a channel for it below ⬇️
                        """);
                }

                List<InteractionComponent> components = [CreateChannelSelect(modMailLog?.ChannelId)];

                if (modMailLog != null)
                {
                    components.Add(CreateStopLoggingButton());
                }

                return new MessageResult(new(new(embed), components));
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public static InteractionComponent CreateChannelSelect(SnowflakeId? defaultChannelId = null)
    {
        return InteractionComponent.CreateChannelSelect(
            custom_id: InteractionCustomId.Create(CustomIdNames.ModMailConfigSetChannel).RawId,
            placeholder: "Pick a Mod Mail channel for this server",
            channel_types: [ChannelType.Text],
            min_values: 1,
            max_values: 1,
            default_values: defaultChannelId != null ? [new SelectDefaultValue($"{defaultChannelId}", "channel")] : null);
    }

    public static InteractionComponent CreateStopLoggingButton()
    {
        return InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Danger,
            custom_id: InteractionCustomId.Create(CustomIdNames.ModMailConfigStop).RawId,
            label: "Disable Mod Mail",
            emoji: new("🗑")));
    }
}

public class ModMailConfigSetChannelHandler(
    IModMailLogChannelRepository modMailLogChannelRepository,
    IInteractionResponseClient responseClient,
    CommandMentioner mention,
    ModMailConfigSlashCommand command) : IChannelSelectHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailConfigSetChannel;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordChannelSelectComponent channelSelect, RunContext context)
    {
        var guild = context.Guild;
        ArgumentNullException.ThrowIfNull(guild);

        var selectedChannel = channelSelect.SelectedChannels.Single();
        var textChannel = new GuildTextChannel(selectedChannel.Id, guild.Id, selectedChannel.Type);

        var modMailLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);

        if (modMailLog != null)
        {
            var content = new MessageContent(EmbedFactory.CreateWarning(
                $"""
                Are you sure you want to change the Mod Mail channel to {selectedChannel.Mention}? ⚠️
                The Mod Mail channel is currently set to {MentionUtils.MentionChannel(modMailLog.ChannelId)} 👈
                Existing messages will **NOT** be moved automatically ✉️
                """));

            var confirmButtonId = InteractionCustomId.Create(CustomIdNames.ModMailConfigConfirm, [new("channel", $"{selectedChannel.Id}")]);
            var prompt = MessageResponse.CreatePrompt(content, confirmButtonId);

            await responseClient.EditOriginalResponseAsync(channelSelect.Interaction, prompt);
        }
        else
        {
            await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(textChannel);

            var embed = EmbedFactory.CreateSuccessEmbed(
                $"""
                Mod Mail is now successfully enabled! ✅
                - Tell your members to send mail with {mention.SlashCommand("modmail message-mods", context)} 📨
                - TaylorBot will send the messages to {selectedChannel.Mention} 📬

                Use {mention.SlashCommand("modmail config", context)} if you need to make some changes ⚙️
                """);

            await responseClient.EditOriginalResponseAsync(channelSelect.Interaction, embed);
        }
    }
}

public class ModMailConfigConfirmHandler(
    IModMailLogChannelRepository modMailLogChannelRepository,
    IInteractionResponseClient responseClient,
    CommandMentioner mention,
    ModMailConfigSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailConfigConfirm;

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
                $"""
                I can't find the channel you picked: {MentionUtils.MentionChannel(channelId)} 😕
                Make sure TaylorBot has access to it! 🤔
                """));
            return;
        }

        var textChannel = new GuildTextChannel(channelId, guild.Id, channel.ChannelType);
        await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(textChannel);

        var embed = EmbedFactory.CreateSuccessEmbed(
            $"""
            Successfully changed the Mod Mail channel to {textChannel.Mention} ✅
            Use {mention.SlashCommand("modmail config", context)} if you need to make some changes ⚙️
            """);

        await responseClient.EditOriginalResponseAsync(button.Interaction, embed);
    }
}

public class ModMailConfigStopHandler(
    IModMailLogChannelRepository modMailLogChannelRepository,
    IInteractionResponseClient responseClient,
    ModMailConfigSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailConfigStop;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        await modMailLogChannelRepository.RemoveModMailLogAsync(context.Guild!);

        var embed = EmbedFactory.CreateSuccess(
            $"""
            Mod Mail is now disabled in the server ✅
            You can re-enable by picking a Mod Mail channel below ⬇️
            """);

        await responseClient.EditOriginalResponseAsync(button.Interaction, new MessageResponse(new([embed]), [ModMailConfigSlashCommand.CreateChannelSelect()]));
    }
}
