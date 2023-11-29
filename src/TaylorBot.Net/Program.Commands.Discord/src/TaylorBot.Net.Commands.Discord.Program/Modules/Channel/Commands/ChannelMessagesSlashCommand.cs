using Discord;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Colors;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Channel.Commands;

public record MessageCount(long Count, bool IsSpam);

public interface IChannelMessageCountRepository
{
    Task<MessageCount> GetMessageCountAsync(ITextChannel channel);
}

public class ChannelMessagesSlashCommand : ISlashCommand<ChannelMessagesSlashCommand.Options>
{
    public ISlashCommandInfo Info => new MessageCommandInfo("channel messages");

    public record Options(ParsedNonThreadTextChannelOrCurrent channel);

    private readonly IChannelMessageCountRepository _channelMessageCountRepository;

    public ChannelMessagesSlashCommand(IChannelMessageCountRepository channelMessageCountRepository)
    {
        _channelMessageCountRepository = channelMessageCountRepository;
    }

    public ValueTask<Command> GetCommandAsync(RunContext context, Options options)
    {
        return new(new Command(
            new(Info.Name),
            async () =>
            {
                var channel = options.channel.Channel;

                var result = await _channelMessageCountRepository.GetMessageCountAsync(channel);

                return new EmbedResult(new EmbedBuilder()
                    .WithGuildAsAuthor(channel.Guild)
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
                        inline: true)
                .Build());
            },
            Preconditions: new ICommandPrecondition[] {
                new InGuildPrecondition(),
            }
        ));
    }
}
