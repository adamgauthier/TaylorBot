using Discord;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

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

                var embed = new EmbedBuilder().WithColor(TaylorBotColors.SuccessColor);

                if (modMailLog != null)
                {
                    var channel = (ITextChannel?)await guild.Fetched.GetChannelAsync(modMailLog.ChannelId.Id);
                    if (channel != null)
                    {
                        embed.WithDescription(
                            $"""
                            ## Mod Mail Configuration 📝
                            This server is currently logging mod mail in {channel.Mention} ✅
                            """);
                    }
                    else
                    {
                        embed.WithDescription(
                            $"""
                            ## Mod Mail Configuration 📝
                            I can't find the configured mod mail channel in this server ⚠️
                            Was it deleted? Does TaylorBot have access? 🛠️
                            You can change it to another channel below ⬇️
                            """);
                    }
                }
                else
                {
                    embed.WithDescription(
                        $"""
                        ## Mod Mail Configuration 📝
                        There is no mod mail logging channel configured in this server ❌
                        Set up a channel to log mod mail conversations below ⬇️
                        """);
                }

                var components = new List<InteractionComponent>();

                if (modMailLog != null)
                {
                    components.Add(CreateStopLoggingButton());
                }

                components.Add(CreateChannelSelect());

                return new MessageResult(new(new(embed.Build()), components));
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public static InteractionComponent CreateChannelSelect()
    {
        return InteractionComponent.CreateChannelSelect(
            custom_id: InteractionCustomId.Create(CustomIdNames.ModMailConfigSetChannel).RawId,
            placeholder: "Select a channel for mod mail",
            channel_types: [ChannelType.Text, ChannelType.News],
            min_values: 1,
            max_values: 1);
    }

    public static InteractionComponent CreateStopLoggingButton()
    {
        return InteractionComponent.CreateActionRow(InteractionComponent.CreateButton(
            style: InteractionButtonStyle.Danger,
            custom_id: InteractionCustomId.Create(CustomIdNames.ModMailConfigStop).RawId,
            label: "Disable mod mail"));
    }
}

public class ModMailConfigSetChannelHandler(
    IModMailLogChannelRepository modMailLogChannelRepository,
    IInteractionResponseClient responseClient,
    ModMailConfigSlashCommand command) : IChannelSelectHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailConfigSetChannel;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordChannelSelectComponent channelSelect, RunContext context)
    {
        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        var selectedChannel = channelSelect.SelectedChannels.Single();
        var textChannel = new GuildTextChannel(selectedChannel.Id, guild.Id, selectedChannel.Type);

        var modMailLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(context.Guild!);

        if (modMailLog != null)
        {
            var content = new MessageContent(new EmbedBuilder()
                .WithColor(TaylorBotColors.WarningColor)
                .WithDescription(
                    $"""
                    Are you sure you want to change the mod mail log channel to {selectedChannel.Mention}? ⚠️
                    Mod mail is currently being logged to {MentionUtils.MentionChannel(modMailLog.ChannelId)} 👈
                    """)
                .Build());

            var confirmButtonId = InteractionCustomId.Create(CustomIdNames.ModMailConfigConfirm, [new("channel", $"{selectedChannel.Id}")]);
            var prompt = MessageResponse.CreatePrompt(content, confirmButtonId);

            await responseClient.EditOriginalResponseAsync(channelSelect.Interaction, prompt);
        }
        else
        {
            await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(textChannel);

            var embed = EmbedFactory.CreateSuccess(
                $"""
                Ok, I will now log mod mail in {selectedChannel.Mention} ✅
                """);

            List<InteractionComponent> components =
            [
                ModMailConfigSlashCommand.CreateStopLoggingButton(),
                ModMailConfigSlashCommand.CreateChannelSelect(),
            ];

            await responseClient.EditOriginalResponseAsync(channelSelect.Interaction, new MessageResponse(new([embed]), components));
        }
    }
}

public class ModMailConfigConfirmHandler(
    IModMailLogChannelRepository modMailLogChannelRepository,
    IInteractionResponseClient responseClient,
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
                $"I can't find the selected channel {MentionUtils.MentionChannel(channelId)}. Make sure TaylorBot has access to it 🤔"));
            return;
        }

        var textChannel = new GuildTextChannel(channelId, guild.Id, channel.ChannelType);
        await modMailLogChannelRepository.AddOrUpdateModMailLogAsync(textChannel);

        var embed = EmbedFactory.CreateSuccess(
            $"""
            Ok, I will now log mod mail in {textChannel.Mention} ✅
            """);

        var components = new List<InteractionComponent>
        {
            ModMailConfigSlashCommand.CreateStopLoggingButton(),
            ModMailConfigSlashCommand.CreateChannelSelect()
        };

        await responseClient.EditOriginalResponseAsync(button.Interaction, new MessageResponse(new([embed]), components));
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
            Ok, I will stop logging mod mail in this server ✅
            """);

        await responseClient.EditOriginalResponseAsync(button.Interaction, new MessageResponse(new([embed]), [ModMailConfigSlashCommand.CreateChannelSelect()]));
    }
}
