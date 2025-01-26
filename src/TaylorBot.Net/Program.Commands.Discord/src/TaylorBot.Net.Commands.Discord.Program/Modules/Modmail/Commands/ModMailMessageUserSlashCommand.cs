using Discord;
using Discord.Net;
using Humanizer;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Modmail.Commands;

public class ModMailMessageUserSlashCommand(Lazy<ITaylorBotClient> client) : ISlashCommand<ModMailMessageUserSlashCommand.Options>
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
                new InGuildPrecondition(botMustBeInGuild: true),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers),
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


public partial class ModMailUserMessageReplyModalHandler(Lazy<ITaylorBotClient> taylorBotClient, InteractionResponseClient responseClient) : IModalHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailUserMessageReplyModal;

    public ModalComponentHandlerInfo Info => new(IsPrivateResponse: true);

    public static readonly Color EmbedColor = new(255, 255, 240);

    public async Task HandleAsync(ModalSubmit submit)
    {
        var messageContent = submit.TextInputs.Single(t => t.CustomId == "messagecontent").Value;

        ArgumentNullException.ThrowIfNull(submit.GuildId);
        var guild = taylorBotClient.Value.ResolveRequiredGuild(submit.GuildId);

        SnowflakeId userId = submit.CustomId.ParsedData["to"];

        var guildUser = await taylorBotClient.Value.ResolveGuildUserAsync(guild.Id, userId);
        if (guildUser == null)
        {
            await responseClient.EditOriginalResponseAsync(submit, message: new(EmbedFactory.CreateError(
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

        var originalMessageText = submit.RawInteraction.message?.embeds?[0]?.description;
        if (originalMessageText != null)
        {
            embed.AddField("In response to", $">>> {originalMessageText}".Truncate(EmbedFieldBuilder.MaxFieldValueLength));
        }

        await responseClient.EditOriginalResponseAsync(
            submit,
            message: new(new([embed.Build(), EmbedFactory.CreateWarning($"Are you sure you want to send the above message to {guildUser.FormatTagAndMention()}?")]),
            [
                new(InteractionCustomId.Create(ModMailReplyConfirmButtonHandler.CustomIdName, submit.CustomId.DataEntries).RawId, Style: ButtonStyle.Success, Label: "Confirm"),
                new(InteractionCustomId.Create(GenericPromptCancelButtonHandler.CustomIdName).RawId, Style: ButtonStyle.Danger, Label: "Cancel"),
            ],
            IsPrivate: false));
    }
}

public partial class ModMailReplyConfirmButtonHandler(
    Lazy<ITaylorBotClient> taylorBotClient,
    InteractionResponseClient responseClient,
    CommandPrefixDomainService commandPrefixDomainService,
    InteractionMapper interactionMapper,
    ModMailChannelLogger modMailChannelLogger,
    IOptionsMonitor<ModMailOptions> modMailOptions) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.ModMailReplyConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(CustomIdName.ToText());

    public async Task HandleAsync(DiscordButtonComponent button)
    {
        // We should automatically build this in the interaction handler
        var context = GetRunContext(button);
        var result = await CanRunAsync(button, context);
        if (result is PreconditionFailed failed)
        {
            await responseClient.EditOriginalResponseAsync(button, message: new(EmbedFactory.CreateError(failed.UserReason.Reason)));
            return;
        }

        var promptMessage = button.RawInteraction.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var modmailEmbed = promptMessage.embeds.First(e => e.title?.Contains("Message from the moderation team", StringComparison.OrdinalIgnoreCase) == true);
        ArgumentNullException.ThrowIfNull(modmailEmbed);

        SnowflakeId userId = button.CustomId.ParsedData["to"];

        ArgumentNullException.ThrowIfNull(button.GuildId);

        var user = await taylorBotClient.Value.ResolveGuildUserAsync(button.GuildId, userId);
        if (user == null)
        {
            await responseClient.EditOriginalResponseAsync(button, message: new(EmbedFactory.CreateError(
                $"""
                Could not find user {MentionUtils.MentionUser(userId)} in this server 😕
                Did they leave? 🤔
                """)));
            return;
        }

        try
        {
            await user.SendMessageAsync(embed: InteractionMapper.ToDiscordEmbed(modmailEmbed));
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            await responseClient.EditOriginalResponseAsync(button, message: new(EmbedFactory.CreateError(
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
                .WithDescription(modmailEmbed.description)
                .WithFooter("Mod mail sent", iconUrl: modMailOptions.CurrentValue.SentLogEmbedFooterIconUrl),
            replyToMessageId
        );

        var resultEmbed = modMailChannelLogger.CreateResultEmbed(context, wasLogged, $"Message sent to {user.FormatTagAndMention()} ✉️");

        await responseClient.EditOriginalResponseAsync(button, message: new(resultEmbed));
    }

    private RunContext GetRunContext(DiscordButtonComponent button)
    {
        var interaction = button.RawInteraction;
        ArgumentNullException.ThrowIfNull(interaction.data);
        ArgumentNullException.ThrowIfNull(interaction.channel_id);
        ArgumentNullException.ThrowIfNull(interaction.channel);

        ApplicationCommand application = new(
            interaction.id,
            interaction.token,
            interaction.data,
            interaction.member != null && interaction.guild_id != null ? new(interaction.guild_id, interaction.member) : null,
            interaction.user,
            (new(interaction.channel_id), interaction.channel));

        return BuildContext(application, null!, wasAcknowledged: true);
    }

    private async Task<ICommandResult> CanRunAsync(DiscordButtonComponent button, RunContext context)
    {
        Command command = new(new($"{CustomIdName}"), RunAsync: null!);

        return await new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers)
            .CanRunAsync(command, context);
    }

    private RunContext BuildContext(ApplicationCommand interaction, CommandActivity activity, bool wasAcknowledged)
    {
        var guild = interaction.Guild != null
            ? taylorBotClient.Value.DiscordShardedClient.GetGuild(interaction.Guild.Id)
            : null;

        var user = interaction.User;

        return new RunContext(
            DateTimeOffset.UtcNow,
            new(
                user.id,
                user.username,
                user.avatar,
                user.discriminator,
                IsBot: user.bot == true,
                interaction.Guild != null
                    ? interactionMapper.ToMemberInfo(interaction.Guild.Id, interaction.Guild.Member)
                    : null),
            FetchedUser: null,
            new(interaction.Channel.Id, (ChannelType)interaction.Channel.Partial.type),
            interaction.Guild != null ? new(interaction.Guild.Id, guild) : null,
            taylorBotClient.Value.DiscordShardedClient,
            taylorBotClient.Value.DiscordShardedClient.CurrentUser,
            new(interaction.Data.id, interaction.Data.name),
            new(() => commandPrefixDomainService.GetPrefixAsync(guild)),
            new(),
            activity,
            WasAcknowledged: wasAcknowledged
        );
    }
}
