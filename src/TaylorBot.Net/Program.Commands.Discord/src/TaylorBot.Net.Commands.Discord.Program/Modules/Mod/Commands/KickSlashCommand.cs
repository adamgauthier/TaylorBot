using Discord;
using Discord.Net;
using Humanizer;
using System.Net;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.Strings;
using TaylorBot.Net.Core.User;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Commands;

public class KickSlashCommand(
    Lazy<ITaylorBotClient> client,
    IModChannelLogger modChannelLogger,
    UserHasPermissionOrOwnerPrecondition.Factory userHasPermission,
    TaylorBotHasPermissionPrecondition.Factory botHasPermission) : ISlashCommand<KickSlashCommand.Options>
{
    private const int MaxAuditLogReasonSize = 512;
    public const string ReasonFieldName = "Kick Reason";

    public static string CommandName => "kick";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName, IsPrivateResponse: true);

    public IList<ICommandPrecondition> BuildPreconditions() => [
        userHasPermission.Create(GuildPermission.KickMembers, GuildPermission.BanMembers),
        botHasPermission.Create(GuildPermission.KickMembers),
    ];

    public record Options(ParsedMemberNotAuthorAndTaylorBot member, ParsedOptionalString reason);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                ArgumentNullException.ThrowIfNull(context.Guild);

                var author = context.FetchedUser != null
                    ? (IGuildUser)context.FetchedUser
                    : await client.Value.ResolveGuildUserAsync(context.Guild.Id, context.User.Id) ?? throw new NotImplementedException();

                var user = options.member.Member;
                var member = await client.Value.ResolveGuildUserAsync(context.Guild.Id, user.User.Id);
                ArgumentNullException.ThrowIfNull(member);

                if (author.Guild.OwnerId == member.Id)
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"You can't kick {member.FormatTagAndMention()} because they're the server owner!"
                    ));
                }
                else if (author.Guild.OwnerId == author.Id || GetHighestRole(member).Position < GetHighestRole(author).Position)
                {
                    if (member.JoinedAt.HasValue && DateTimeOffset.UtcNow - member.JoinedAt.Value > TimeSpan.FromDays(30))
                    {
                        var embed = new EmbedBuilder()
                            .WithColor(TaylorBotColors.WarningColor)
                            .WithDescription(
                                $"""
                                {member.FormatTagAndMention()} joined the server **{member.JoinedAt.Value.Humanize(culture: TaylorBotCulture.Culture)}** ⚠️
                                Are you sure you want to kick?
                                """);

                        if (!string.IsNullOrWhiteSpace(options.reason.Value))
                        {
                            embed.AddField(ReasonFieldName, options.reason.Value);
                        }

                        return MessageResult.CreatePrompt(
                            new(embed.Build()),
                            InteractionCustomId.Create(CustomIdNames.KickConfirm, [new("user", $"{member.Id}")])
                        );
                    }
                    else
                    {
                        var embed = await KickAsync(context, options.reason.Value, new(author), member);
                        return new EmbedResult(embed);
                    }
                }
                else
                {
                    return new EmbedResult(EmbedFactory.CreateError(
                        $"""
                        You can't kick {member.FormatTagAndMention()} because their highest role is equal to or higher than yours in the roles list.
                        The order of roles in server settings is important, you can only kick someone whose role is lower than yours.
                        """));
                }
            },
            Preconditions: BuildPreconditions()
        ));
    }

    public async ValueTask<Embed> KickAsync(RunContext context, string? reason, DiscordUser author, IGuildUser member)
    {
        try
        {
            await member.KickAsync($"{author.FormatTagAndMention()} used /kick{(!string.IsNullOrEmpty(reason) ? $": {reason}" : " (no reason specified)")}".Truncate(MaxAuditLogReasonSize));
        }
        catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
        {
            return EmbedFactory.CreateError(
                $"""
                Could not kick {member.FormatTagAndMention()} due to missing permissions.
                In server settings, make sure TaylorBot's role is **higher in the list** than this member's roles.
                """);
        }

        var wasLogged = await modChannelLogger.TrySendModLogAsync(member.Guild, author, new(member), logEmbed =>
        {
            if (!string.IsNullOrEmpty(reason))
                logEmbed.AddField("Reason", reason);

            return logEmbed
                .WithColor(new(222, 184, 135))
                .WithFooter("User kicked");
        });

        return modChannelLogger.CreateResultEmbed(context, wasLogged, $"{member.FormatTagAndMention()} was successfully kicked 👢");
    }

    private static IRole GetHighestRole(IGuildUser member)
    {
        return member.Guild.Roles.Where(r => member.RoleIds.Contains(r.Id)).OrderByDescending(r => r.Position).First();
    }
}

public class KickConfirmButtonHandler(Lazy<ITaylorBotClient> client, InteractionResponseClient responseClient, KickSlashCommand command) : IButtonHandler
{
    public static CustomIdNames CustomIdName => CustomIdNames.KickConfirm;

    public IComponentHandlerInfo Info => new MessageHandlerInfo(
        CustomIdName.ToText(),
        Preconditions: command.BuildPreconditions(),
        RequireOriginalUser: true);

    public async Task HandleAsync(DiscordButtonComponent button, RunContext context)
    {
        var promptMessage = button.Interaction.Raw.message;
        ArgumentNullException.ThrowIfNull(promptMessage);

        var reason = promptMessage.embeds[0].fields?.First(f => f.name.Contains(KickSlashCommand.ReasonFieldName, StringComparison.OrdinalIgnoreCase) == true)?.value;

        var userId = button.CustomId.ParsedData["user"];

        ArgumentNullException.ThrowIfNull(context.Guild);
        var member = await client.Value.ResolveGuildUserAsync(context.Guild.Id, userId);
        if (member == null)
        {
            await responseClient.EditOriginalResponseAsync(button.Interaction, EmbedFactory.CreateErrorEmbed(
                $"Can't find {MentionUtils.MentionUser(new SnowflakeId(userId))} in this server 😕"));
            return;
        }

        var embed = await command.KickAsync(context, reason, context.User, member);

        await responseClient.EditOriginalResponseAsync(button.Interaction, InteractionMapper.ToInteractionEmbed(embed));
    }
}
