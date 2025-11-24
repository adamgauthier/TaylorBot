using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailMessageModsSlashCommand(InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static string CommandName => "modmail message-mods";

    public ISlashCommandInfo Info => new ModalCommandInfo(CommandName);

    public IList<ICommandPrecondition> BuildPreconditions() => [inGuild.Create(botMustBeInGuild: true)];

    public static readonly Color EmbedColor = new(255, 255, 240);
    public const string EmbedFooterText = "Mod mail received";

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                return new(new CreateModalResult(
                    Id: InteractionCustomId.Create(ModMailMessageModsModalHandler.CustomIdName).RawId,
                    Title: "Send Message to Moderators",
                    TextInputs: [
                        new TextInput(Id: "subject", Style: TextInputStyle.Short, Label: "Subject", Required: false, MaxLength: 50),
                        new TextInput(Id: "messagecontent", Style: TextInputStyle.Paragraph, Label: "Message to moderators", Required: true)
                    ]));
            },
            Preconditions: BuildPreconditions()
        ));
    }
}

public class ModMailMessageModsModalHandler(
    IInteractionResponseClient responseClient,
    IOptionsMonitor<ModMailOptions> modMailOptions,
    ModMailMessageModsSlashCommand command,
    IModMailLogChannelRepository modMailLogChannelRepository,
    ModMailChannelLogger modMailChannelLogger) : IModalHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailMessageModsModal;

    public ModalComponentHandlerInfo Info => new(IsPrivateResponse: true, Preconditions: command.BuildPreconditions());

    public async Task HandleAsync(ModalSubmit submit, RunContext context)
    {
        var subject = submit.TextInputs.Single(t => t.CustomId == "subject").Value;
        if (string.IsNullOrWhiteSpace(subject))
        {
            subject = "No Subject";
        }

        var messageContent = submit.TextInputs.Single(t => t.CustomId == "messagecontent").Value;

        var guild = context.Guild;
        ArgumentNullException.ThrowIfNull(guild);

        var modLog = await modMailLogChannelRepository.GetModMailLogForGuildAsync(guild);
        if (modLog == null)
        {
            await responseClient.EditOriginalResponseAsync(submit.Interaction, new MessageResponse(modMailChannelLogger.CreateNotConfiguredModMailLogEmbed(context)));
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(ModMailMessageModsSlashCommand.EmbedColor)
            .WithTitle(subject)
            .WithDescription(messageContent)
            .AddField("From", submit.Interaction.User.FormatTagAndMention(), inline: true)
            .WithFooter(ModMailMessageModsSlashCommand.EmbedFooterText, iconUrl: modMailOptions.CurrentValue.ReceivedLogEmbedFooterIconUrl)
            .WithCurrentTimestamp()
            .Build();

        await responseClient.EditOriginalResponseAsync(
            submit.Interaction,
            MessageResponse.CreatePrompt(
                new([
                    embed,
                    EmbedFactory.CreateWarning($"Are you sure you want to send the above message to the moderation team of this server?")]),
                InteractionCustomId.Create(ModMailMessageModsConfirmButtonHandler.CustomIdName))
        );
    }
}

public partial class ModMailMessageModsConfirmButtonHandler(
    ModMailMessageModsSlashCommand command,
    IInteractionResponseClient responseClient,
    ILogger<ModMailMessageModsConfirmButtonHandler> logger,
    IModMailBlockedUsersRepository modMailBlockedUsersRepository,
    ModMailChannelLogger modMailChannelLogger) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailMessageModsConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var modMailEmbed = promptMessage.embeds.First(e => e.footer?.text.Contains(ModMailMessageModsSlashCommand.EmbedFooterText, StringComparison.OrdinalIgnoreCase) == true);

        var guild = context.Guild?.Fetched;
        ArgumentNullException.ThrowIfNull(guild);

        var isBlocked = await modMailBlockedUsersRepository.IsBlockedAsync(guild, context.User);
        if (isBlocked)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                "Sorry, the moderation team has blocked you from sending mod mail 😕"));
            return;
        }

        var result = await modMailChannelLogger.GetModMailLogAsync(guild, context);
        if (!result.IsSuccess)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, new MessageResponse(result.Error));
            return;
        }

        try
        {
            await result.Value.SendMessageAsync(embed: InteractionMapper.ToDiscordEmbed(modMailEmbed), components: new ComponentBuilder()
                .WithButton(
                    customId: InteractionCustomId.Create(ModMailUserMessageReplyButtonHandler.CustomIdName, [new("to", context.User.Id)]).RawId,
                    label: "Reply", emote: new Emoji("📨"))
                .Build());

            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateSuccessEmbed(
                $"""
                    Message sent to the moderation team of '{guild.Name}' ✉️
                    If you're expecting a response, **make sure you're able to send & receive DMs from TaylorBot** ⚙️
                    """));
            return;
        }
        catch (Exception e)
        {
            LogErrorSendingModMail(e, guild.FormatLog());
        }

        await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
            $"""
            Failed to send message to the moderation team because I couldn't access the mod mail log channel 😕
            Ask a moderator to check if TaylorBot has the necessary permissions in the channel 🛠️
            """));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error occurred when sending mod mail in {Guild}:")]
    private partial void LogErrorSendingModMail(Exception exception, string guild);
}

public class ModMailUserMessageReplyButtonHandler(IInteractionResponseClient responseClient) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailUserMessageReply;

    public IComponentHandlerInfo Info => new ModalHandlerInfo(CustomIdName.ToText());

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var toUserId = button.CustomId.ParsedData["to"];
        await responseClient.SendModalResponseAsync(button, ModMailMessageUserSlashCommand.CreateModalResult(toUserId, button.Message.id));
    }
}
