using Discord;
using FakeItEasy;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands;

public record MessageChannel(string Id)
{
    public string Mention => MentionUtils.MentionChannel(new SnowflakeId(Id).Id);

    public ITextChannel CreateLegacyTextChannel(IGuild guild)
    {
        var textChannel = A.Fake<ITextChannel>(o => o.Strict());
        A.CallTo(() => textChannel.Id).Returns(new SnowflakeId(Id).Id);
        A.CallTo(() => textChannel.GuildId).Returns(guild.Id);
        return textChannel;
    }
}

public record RunContext(
    DateTimeOffset CreatedAt,
    IUser User,
    MessageChannel Channel,
    IGuild? Guild,
    IDiscordClient Client,
    ISelfUser BotUser,
    RunContext.CurrentCommandInfo CommandInfo,
    string CommandPrefix,
    RunContext.OnGoingState OnGoing,
    bool WasAcknowledged = true,
    bool IsFakeGuild = false
)
{
    public record CurrentCommandInfo(string Id, string Name);
    public class OnGoingState { public string? OnGoingCommandAddedToPool { get; set; } }

    public string MentionCommand(string name) =>
        name.Split(' ')[0] == CommandInfo.Name.Split(' ')[0] ?
            $"</{name}:{CommandInfo.Id}>" :
            $"**/{name}**";
}

public record Command(CommandMetadata Metadata, Func<ValueTask<ICommandResult>> RunAsync, IList<ICommandPrecondition>? Preconditions = null);
public record CommandMetadata(string Name, string? ModuleName = null, IReadOnlyList<string>? Aliases = null);

public interface ICommandRunner
{
    ValueTask<ICommandResult> RunAsync(Command command, RunContext context);
}

public class CommandRunner : ICommandRunner
{
    private readonly List<ICommandPrecondition> _preconditions;

    public CommandRunner(
        NotDisabledPrecondition notDisabled,
        NotGuildDisabledPrecondition notGuildDisabled,
        NotGuildChannelDisabledPrecondition notGuildChannelDisabled,
        UserNotIgnoredPrecondition userNotIgnored,
        MemberTrackedPrecondition memberTracked,
        TextChannelTrackedPrecondition textChannelTracked,
        UserNoOngoingCommandPrecondition userNoOngoingCommand
    )
    {
        _preconditions = new() { notDisabled, notGuildDisabled, notGuildChannelDisabled, userNotIgnored, memberTracked, textChannelTracked, userNoOngoingCommand };
    }

    public async ValueTask<ICommandResult> RunAsync(Command command, RunContext context)
    {
        foreach (var precondition in _preconditions.Concat(command.Preconditions ?? Array.Empty<ICommandPrecondition>()))
        {
            if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                return failed;
        }

        var result = await command.RunAsync();

        return result;
    }
}
