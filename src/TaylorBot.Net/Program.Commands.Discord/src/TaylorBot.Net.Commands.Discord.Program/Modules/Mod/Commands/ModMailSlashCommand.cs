using Discord;
using Discord.Net;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class ModMailMessageUserSlashCommand : ISlashCommand<ModMailMessageUserSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("mod mail message-user", IsPrivateResponse: true);

        public record Options(ParsedMemberNotAuthorAndTaylorBot user, ParsedString message);

        private static readonly Color EmbedColor = new(240, 255, 240);

        private readonly IModChannelLogger _modChannelLogger;

        public ModMailMessageUserSlashCommand(IModChannelLogger modChannelLogger)
        {
            _modChannelLogger = modChannelLogger;
        }

        public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
        {
            return new(new Command(
                new(Info.Name),
                async () =>
                {
                    var guild = context.Guild!;
                    var user = options.user.Member;
                    var channel = await _modChannelLogger.GetModLogAsync(guild);

                    if (channel == null)
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            "It looks like the moderation log channel is not set for this server. ❌",
                            "This is required to send mod mail to make sure other moderators can see what message has been sent.",
                            "Use `/mod log set` to set up moderation command usage logging in this server."
                        })));

                    var embed = new EmbedBuilder()
                        .WithGuildAsAuthor(guild)
                        .WithColor(EmbedColor)
                        .WithTitle("Message from the moderation team")
                        .WithDescription(options.message.Value)
                    .Build();

                    return new PromptEmbedResult(
                        new(new[] { embed, EmbedFactory.CreateWarning($"Are you sure you want to send the above message to {user.FormatTagAndMention()}?") }),
                        Confirm: async () => new(await SendAsync(channel))
                    );

                    async ValueTask<Embed> SendAsync(ITextChannel channel)
                    {
                        try
                        {
                            await user.SendMessageAsync(embed: embed);
                        }
                        catch (HttpException e) when (e.DiscordCode == 50007)
                        {
                            return EmbedFactory.CreateError(string.Join('\n', new[] {
                                $"I couldn't send this message to {user.FormatTagAndMention()} because their DM settings won't allow it. ❌",
                                "The user must have 'Allow direct messages from server members' enabled and must not have TaylorBot blocked."
                            }));
                        }

                        await _modChannelLogger.TrySendModLogAsync(channel, context.User, user, logEmbed =>
                            logEmbed
                                .WithColor(EmbedColor)
                                .AddField("Message", options.message.Value)
                                .WithFooter("Mod mail sent")
                        );

                        return EmbedFactory.CreateSuccess($"✉ Message sent to {user.FormatTagAndMention()}");
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
