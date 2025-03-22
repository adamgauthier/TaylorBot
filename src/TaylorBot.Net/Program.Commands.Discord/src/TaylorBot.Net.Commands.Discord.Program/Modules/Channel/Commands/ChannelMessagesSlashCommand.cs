using Discord;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Commands;

public record MessageCount(long Count, bool IsSpam);

public interface IChannelMessageCountRepository
{
    Task<MessageCount> GetMessageCountAsync(GuildTextChannel channel);
}

public class ChannelMessagesSlashCommand(
    IChannelMessageCountRepository channelMessageCountRepository,
    InGuildPrecondition.Factory inGuild) : ISlashCommand<ChannelMessagesSlashCommand.Options>
{
    public static string CommandName => "channel messages";

    public ISlashCommandInfo Info => new MessageCommandInfo(CommandName);

    public record Options(ParsedTextChannelOrCurrent channel);

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;

                var result = await channelMessageCountRepository.GetMessageCountAsync(channel);

                var embed = new EmbedBuilder()
                    .WithColor(TaylorBotColors.SuccessColor)
                    .AddField(
                        "Message Count",
                        $"""
                        **~{result.Count.ToString(TaylorBotFormats.Readable)}**
                        This message count is **approximate** and is updated every few minutes.
                        This is tracked regardless of the spam status of this channel.
                        """, inline: true)
                    .AddField(
                        "Spam Status",
                        result.IsSpam ?
                            """
                            This channel is considered as spam. Users' messages and words are **NOT** counted. 🐟
                             Use </mod spam remove:838266590294048778> to mark the channel as non-spam.
                            """ :
                            """
                            This channel is not considered as spam. Users' messages and words are counted. ✅
                            Use </mod spam add:838266590294048778> to mark the channel as spam.
                            """,
                        inline: true);

                if (context.Guild?.Fetched != null)
                {
                    embed.WithGuildAsAuthor(context.Guild.Fetched);
                }

                return new EmbedResult(embed.Build());
            },
            Preconditions: [inGuild.Create()]
        ));
    }
}
