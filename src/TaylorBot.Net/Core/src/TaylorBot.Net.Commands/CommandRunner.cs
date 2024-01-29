using Discord;
using FakeItEasy;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Snowflake;

namespace TaylorBot.Net.Commands;

public record MessageChannel(SnowflakeId Id)
{
    public string Mention => MentionUtils.MentionChannel(Id);

    public ITextChannel CreateLegacyTextChannel(IGuild guild)
    {
        var textChannel = A.Fake<ITextChannel>(o => o.Strict());
        A.CallTo(() => textChannel.Id).Returns(Id);
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
    CommandActivity Activity,
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

public class CommandRunner(
    NotDisabledPrecondition notDisabled,
    NotGuildDisabledPrecondition notGuildDisabled,
    NotGuildChannelDisabledPrecondition notGuildChannelDisabled,
    UserNotIgnoredPrecondition userNotIgnored,
    MemberTrackedPrecondition memberTracked,
    TextChannelTrackedPrecondition textChannelTracked,
    UserNoOngoingCommandPrecondition userNoOngoingCommand
    ) : ICommandRunner
{
    private readonly List<ICommandPrecondition> _preconditions = [notDisabled, notGuildDisabled, notGuildChannelDisabled, userNotIgnored, memberTracked, textChannelTracked, userNoOngoingCommand];

    public async ValueTask<ICommandResult> RunAsync(Command command, RunContext context)
    {
        context.Activity.CommandName = command.Metadata.Name;
        context.Activity.UserId = context.User.Id;
        context.Activity.ChannelId = context.Channel.Id;
        context.Activity.GuildId = context.Guild?.Id;

        foreach (var precondition in _preconditions.Concat(command.Preconditions ?? Array.Empty<ICommandPrecondition>()))
        {
            if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                return failed;
        }

        var result = await command.RunAsync();

        return result;
    }
}
