using Discord;
using System.Threading.Tasks;
using TaylorBot.Net.Commands.Discord.Program.Modules.Mod.Domain;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Strings;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.DiscordInfo.Commands
{
    public class ModMailMessageModsSlashCommand : ISlashCommand<ModMailMessageModsSlashCommand.Options>
    {
        public SlashCommandInfo Info => new("mod mail message-mods", IsPrivateResponse: true);

        public record Options(ParsedString message);

        private static readonly Color EmbedColor = new(255, 255, 240);

        private readonly IModChannelLogger _modChannelLogger;

        public ModMailMessageModsSlashCommand(IModChannelLogger modChannelLogger)
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
                    var channel = await _modChannelLogger.GetModLogAsync(guild);

                    if (channel == null)
                        return new EmbedResult(EmbedFactory.CreateError(string.Join('\n', new[] {
                            "It looks like the moderation team has not set up a moderation log channel. ❌",
                            "This is required to receive mod mail. Ask the moderators to use `/mod log set`.",
                        })));

                    var embed = new EmbedBuilder()
                        .WithColor(EmbedColor)
                        .AddField("From", context.User.FormatTagAndMention(), inline: true)
                        .AddField("Message", options.message.Value)
                        .WithFooter("Mod mail received")
                    .Build();

                    return new PromptEmbedResult(
                        new(new[] { embed, EmbedFactory.CreateWarning($"Are you sure you want to send the above message to the moderation team of '{guild.Name}'?") }),
                        Confirm: async () => new(await SendAsync(channel))
                    );

                    async ValueTask<Embed> SendAsync(ITextChannel channel)
                    {
                        await channel.SendMessageAsync(embed: embed);

                        return EmbedFactory.CreateSuccess($"✉ Message sent to the moderation team of '{guild.Name}'.");
                    }
                },
                Preconditions: new ICommandPrecondition[] {
                    new InGuildPrecondition()
                }
            ));
        }
    }
}
