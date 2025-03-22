using Discord;
using Discord.Net;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailMessageUserSlashCommand(
    Lazy<ITaylorBotClient> client,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<ModMailMessageUserSlashCommand.Options>
{
    public static string CommandName => "modmail message-user";

    public ISlashCommandInfo Info => new ModalCommandInfo(CommandName);

    public record Options(ParsedMemberNotAuthorAndBot user);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var guild = context.Guild?.Fetched;
                ArgumentNullException.ThrowIfNull(guild);

                var user = options.user.Member;
                var guildUser = await client.Value.ResolveGuildUserAsync(guild.Id, user.User.Id);
                ArgumentNullException.ThrowIfNull(guildUser);

                return CreateModalResult(to: user.User.Id);
            },
            Preconditions: [
                inGuild.Create(botMustBeInGuild: true),
                userHasPermission.Create(GuildPermission.BanMembers),
            ]
        ));
    }

    public static CreateModalResult CreateModalResult(SnowflakeId to, SnowflakeId? replyToMessageId = null)
    {
        List<CustomIdDataEntry> data = [new("to", to)];
        if (replyToMessageId != null)
        {
            data.Add(new("rep", replyToMessageId));
        }

        return new(
            Id: InteractionCustomId.Create(ModMailUserMessageReplyModalHandler.CustomIdName, data).RawId,
            Title: "Send Mod Mail to User",
            TextInputs: [new TextInput(Id: "messagecontent", TextInputStyle.Paragraph, Label: "Message to user")],
            SubmitAction: null,
            IsPrivateResponse: true
        );
    }
}


public class ModMailUserMessageReplyModalHandler(
    Lazy<ITaylorBotClient> taylorBotClient,
    InteractionResponseClient responseClient,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission) : IModalHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailUserMessageReplyModal;

    public ModalComponentHandlerInfo Info => new(
        IsPrivateResponse: true,
        Preconditions: [userHasPermission.Create(GuildPermission.BanMembers)]);

    public static readonly Color EmbedColor = new(255, 255, 240);

    public async Task HandleAsync(ModalSubmit submit)
    {
        var messageContent = submit.TextInputs.Single(t => t.CustomId == "messagecontent").Value;

        ArgumentNullException.ThrowIfNull(submit.Interaction.Guild);
        var guild = taylorBotClient.Value.ResolveRequiredGuild(submit.Interaction.Guild.Id);

        SnowflakeId userId = submit.CustomId.ParsedData["to"];

        var guildUser = await taylorBotClient.Value.ResolveGuildUserAsync(guild.Id, userId);
        if (guildUser == null)
        {
            await responseClient.EditOriginalResponseAsync(submit.Interaction, message: new(EmbedFactory.CreateError(
                $"""
                Could not find user {MentionUtils.MentionUser(userId)} in this server 😕
                Did they leave? 🤔
                """)));
            return;
        }

        var embed = new EmbedBuilder()
            .WithGuildAsAuthor(guild)
            .WithColor(EmbedColor)
            .WithTitle("Message from the moderation team")
            .WithDescription(messageContent)
            .WithFooter("Reply with /modmail message-mods");

        var originalMessageText = submit.Interaction.Raw.message?.embeds?[0]?.description;
        if (originalMessageText != null)
        {
            embed.AddField("In response to", $">>> {originalMessageText}".Truncate(EmbedFieldBuilder.MaxFieldValueLength));
        }

        await responseClient.EditOriginalResponseAsync(
            submit.Interaction,
            message: new(new([embed.Build(), EmbedFactory.CreateWarning($"Are you sure you want to send the above message to {guildUser.FormatTagAndMention()}?")]),
            [
                new(InteractionCustomId.Create(ModMailReplyConfirmButtonHandler.CustomIdName, submit.CustomId.DataEntries).RawId, Style: ButtonStyle.Success, Label: "Confirm"),
                new(InteractionCustomId.Create(GenericPromptCancelButtonHandler.CustomIdName).RawId, Style: ButtonStyle.Danger, Label: "Cancel"),
            ],
            IsPrivate: false));
    }
}

public class ModMailReplyConfirmButtonHandler(
    Lazy<ITaylorBotClient> taylorBotClient,
    InteractionResponseClient responseClient,
    ModMailChannelLogger modMailChannelLogger,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    IOptionsMonitor<ModMailOptions> modMailOptions) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailReplyConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText(), Preconditions: [userHasPermission.Create(GuildPermission.BanMembers)], RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var modMailEmbed = promptMessage.embeds.First(e => e.title?.Contains("Message from the moderation team", StringComparison.OrdinalIgnoreCase) == true);
        ArgumentNullException.ThrowIfNull(modMailEmbed);

        SnowflakeId userId = button.CustomId.ParsedData["to"];

        ArgumentNullException.ThrowIfNull(button.Interaction.Guild);

        var user = await taylorBotClient.Value.ResolveGuildUserAsync(button.Interaction.Guild.Id, userId);
        if (user == null)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(EmbedFactory.CreateError(
                $"""
                Could not find user {MentionUtils.MentionUser(userId)} in this server 😕
                Did they leave? 🤔
                """)));
            return;
        }

        try
        {
            await user.SendMessageAsync(embed: InteractionMapper.ToDiscordEmbed(modMailEmbed));
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(EmbedFactory.CreateError(
                $"""
                Could not send message to {user.FormatTagAndMention()} because their DM settings won't allow it ❌
                The user must have 'Allow direct messages from server members' enabled and must not have TaylorBot blocked ⚙️
                """)));
            return;
        }

        SnowflakeId? replyToMessageId = null;
        if (button.CustomId.ParsedData.TryGetValue("rep", out var rep) && !string.IsNullOrWhiteSpace(rep))
        {
            replyToMessageId = rep;
        }

        var wasLogged = await modMailChannelLogger.TrySendModMailLogAsync(
            user.Guild,
            context.User,
            new(user),
            logEmbed => logEmbed
                .WithColor(ModMailUserMessageReplyModalHandler.EmbedColor)
                .WithTitle("Message")
                .WithDescription(modMailEmbed.description)
                .WithFooter("Mod mail sent", iconUrl: modMailOptions.CurrentValue.SentLogEmbedFooterIconUrl),
            replyToMessageId
        );

        var resultEmbed = modMailChannelLogger.CreateResultEmbed(context, wasLogged, $"Message sent to {user.FormatTagAndMention()} ✉️");

        await responseClient.EditOriginalResponseAsync(button.Interaction, message: new(resultEmbed));
    }
}
