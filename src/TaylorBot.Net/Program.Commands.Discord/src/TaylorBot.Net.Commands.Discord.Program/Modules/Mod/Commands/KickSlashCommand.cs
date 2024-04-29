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
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands;

public class KickSlashCommand(Lazy<ITaylorBotClient> client, IModChannelLogger modChannelLogger) : ISlashCommand<KickSlashCommand.Options>
{
    private const int MaxAuditLogReasonSize = 512;

    public ISlashCommandInfo Info => new MessageCommandInfo("kick", IsPrivateResponse: true);

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
                    if (member.JoinedAt.HasValue && (DateTimeOffset.Now - member.JoinedAt.Value) > TimeSpan.FromDays(30))
                    {
                        return MessageResult.CreatePrompt(
                            new(EmbedFactory.CreateWarning(
                                $"""
                                {member.FormatTagAndMention()} joined the server **{member.JoinedAt.Value.Humanize(culture: TaylorBotCulture.Culture)}**.
                                Are you sure you want to kick?
                                """)),
                            confirm: async () => new(await KickAsync())
                        );
                    }
                    else
                    {
                        var embed = await KickAsync();
                        return new EmbedResult(embed);
                    }

                    async ValueTask<Embed> KickAsync()
                    {
                        try
                        {
                            await member.KickAsync($"{author.FormatLog()} used /kick{(!string.IsNullOrEmpty(options.reason.Value) ? $": {options.reason.Value}" : " (No reason specified)")}".Truncate(MaxAuditLogReasonSize));
                        }
                        catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                        {
                            return EmbedFactory.CreateError(
                                $"""
                                Could not kick {member.FormatTagAndMention()} due to missing permissions.
                                In server settings, make sure TaylorBot's role is **higher in the list** than this member's roles.
                                """);
                        }

                        var wasLogged = await modChannelLogger.TrySendModLogAsync(member.Guild, author, member, logEmbed =>
                        {
                            if (!string.IsNullOrEmpty(options.reason.Value))
                                logEmbed.AddField("Reason", options.reason.Value);

                            return logEmbed
                                .WithColor(new(222, 184, 135))
                                .WithFooter("User kicked");
                        });

                        return modChannelLogger.CreateResultEmbed(context, wasLogged, $"{member.FormatTagAndMention()} was successfully kicked. 👢");
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
            Preconditions: [
                new InGuildPrecondition(),
                new UserHasPermissionOrOwnerPrecondition(GuildPermission.KickMembers, GuildPermission.BanMembers),
                new TaylorBotHasPermissionPrecondition(GuildPermission.KickMembers)
            ]
        ));
    }

    private static IRole GetHighestRole(IGuildUser member)
    {
        return member.Guild.Roles.Where(r => member.RoleIds.Contains(r.Id)).OrderByDescending(r => r.Position).First();
    }
}
