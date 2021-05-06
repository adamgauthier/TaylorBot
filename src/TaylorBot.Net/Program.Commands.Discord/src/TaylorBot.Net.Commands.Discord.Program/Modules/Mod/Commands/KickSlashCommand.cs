using Discord;
using Humanizer;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Events;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class KickSlashCommand : ISlashCommand<KickSlashCommand.Options>
    {
        private const int MaxAuditLogReasonSize = 512;

        public string Name => "kick";

        public record Options(ParsedMemberNotAuthor member, ParsedOptionalString reason);

        private readonly IModChannelLogger _modChannelLogger;

        public KickSlashCommand(IModChannelLogger modChannelLogger)
        {
            _modChannelLogger = modChannelLogger;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Name),
                async () =>
                {
                    var member = options.member.Member;

                    await member.KickAsync($"{context.User.FormatLog()} used /kick{(!string.IsNullOrEmpty(options.reason.Value) ? $": {options.reason.Value}" : " (No reason specified)")}".Truncate(MaxAuditLogReasonSize));

                    await _modChannelLogger.TrySendModLogAsync(context.Guild!, context.User, member, logEmbed =>
                    {
                        if (!string.IsNullOrEmpty(options.reason.Value))
                            logEmbed.AddField("Reason", options.reason.Value);

                        return logEmbed
                            .WithColor(new(222, 184, 135))
                            .WithFooter("User kicked");
                    });

                    return new EmbedResult(new EmbedBuilder()
                        .WithColor(TaylorBotColors.SuccessColor)
                        .WithDescription($"👢 Kicked {member.FormatTagAndMention()}")
                    .Build());
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.KickMembers),
                    new TaylorBotHasPermissionPrecondition(GuildPermission.KickMembers)
                }
            ));
        }
    }
}
