using Discord;
using Discord.Net;
using Humanizer;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Globalization;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class KickSlashCommand : ISlashCommand<KickSlashCommand.Options>
    {
        private const int MaxAuditLogReasonSize = 512;

        public SlashCommandInfo Info => new("kick", IsPrivateResponse: true);

        public record Options(ParsedMemberNotAuthorAndTaylorBot member, ParsedOptionalString reason);

        private readonly IModChannelLogger _modChannelLogger;

        public KickSlashCommand(IModChannelLogger modChannelLogger)
        {
            _modChannelLogger = modChannelLogger;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var author = (IGuildUser)context.User;
                    var member = options.member.Member;

                    if (member.Guild.OwnerId == member.Id)
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
                                new(EmbedFactory.CreateWarning(string.Join('\n', new[] {
                                    $"{member.FormatTagAndMention()} joined the server **{member.JoinedAt.Value.Humanize(culture: TaylorBotCulture.Culture)}**.",
                                    "Are you sure you want to kick?"
                                }))),
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
                                return EmbedFactory.CreateError(string.Join('\n', new[] {
                                    $"Could not kick {member.FormatTagAndMention()} due to missing permissions.",
                                    "In server settings, make sure TaylorBot's role is **higher in the list** than this member's roles."
                                }));
                            }

                            var wasLogged = await _modChannelLogger.TrySendModLogAsync(member.Guild, author, member, logEmbed =>
                            {
                                if (!string.IsNullOrEmpty(options.reason.Value))
                                    logEmbed.AddField("Reason", options.reason.Value);

                                return logEmbed
                                    .WithColor(new(222, 184, 135))
                                    .WithFooter("User kicked");
                            });

                            return _modChannelLogger.CreateResultEmbed(context, wasLogged, $"{member.FormatTagAndMention()} was successfully kicked. 👢");
                        }
                    }
                    else
                    {
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            $"You can't kick {member.FormatTagAndMention()} because their highest role is equal to or higher than yours in the roles list.",
                            $"The order of roles in server settings is important, you can only kick someone whose role is lower than yours."
                        })));
                    }
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.KickMembers, GuildPermission.BanMembers),
                    new TaylorBotHasPermissionPrecondition(GuildPermission.KickMembers)
                }
            ));
        }

        private static IRole GetHighestRole(IGuildUser member)
        {
            return member.Guild.Roles.Where(r => member.RoleIds.Contains(r.Id)).OrderByDescending(r => r.Position).First();
        }
    }
}
