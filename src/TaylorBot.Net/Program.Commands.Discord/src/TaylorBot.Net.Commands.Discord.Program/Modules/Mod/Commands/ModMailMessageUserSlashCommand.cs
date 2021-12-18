using Discord;
using Discord.Net;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Discord.Program.Options;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.Parsers.Users;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class ModMailMessageUserSlashCommand : ISlashCommand<ModMailMessageUserSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("mod mail message-user", IsPrivateResponse: true);

        public record Options(ParsedMemberNotAuthorAndBot user, ParsedString message);

        private static readonly Color EmbedColor = new(255, 255, 240);

        private readonly IModChannelLogger _modChannelLogger;
        private readonly IOptionsMonitor<ModMailOptions> _options;

        public ModMailMessageUserSlashCommand(IModChannelLogger modChannelLogger, IOptionsMonitor<ModMailOptions> options)
        {
            _modChannelLogger = modChannelLogger;
            _options = options;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                () =>
                {
                    var guild = context.Guild!;
                    var user = options.user.Member;

                    var embed = new EmbedBuilder()
                        .WithGuildAsAuthor(guild)
                        .WithColor(EmbedColor)
                        .WithTitle("Message from the moderation team")
                        .WithDescription(options.message.Value)
                        .WithFooter("Reply with /mod mail message-mods")
                    .Build();

                    return new(MessageResult.CreatePrompt(
                        new(new[] { embed, EmbedFactory.CreateWarning($"Are you sure you want to send the above message to {user.FormatTagAndMention()}?") }),
                        confirm: async () => new(await SendAsync())
                    ));

                    async ValueTask<Embed> SendAsync()
                    {
                        try
                        {
                            await user.SendMessageAsync(embed: embed);
                        }
                        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                        {
                            return EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"I couldn't send this message to {user.FormatTagAndMention()} because their DM settings won't allow it. ❌",
                                "The user must have 'Allow direct messages from server members' enabled and must not have TaylorBot blocked."
                            }));
                        }

                        var wasLogged = await _modChannelLogger.TrySendModLogAsync(guild, context.User, user, logEmbed =>
                            logEmbed
                                .WithColor(EmbedColor)
                                .AddField("Message", options.message.Value)
                                .WithFooter("Mod mail sent", iconUrl: _options.CurrentValue.SentLogEmbedFooterIconUrl)
                        );

                        return _modChannelLogger.CreateResultEmbed(wasLogged, $"Message sent to {user.FormatTagAndMention()}. ✉");
                    }
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition(),
                    new UserHasPermissionOrOwnerPrecondition(GuildPermission.BanMembers)
                }
            ));
        }
    }
}
