using Discord;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands;

public record CommandChannel(SnowflakeId Id, ChannelType Type)
{
    public string Mention => MentionUtils.MentionChannel(Id);
}

public record CommandGuild(SnowflakeId Id, IGuild? Fetched)
{
    public string FormatLog()
    {
        return Fetched != null ? Fetched.FormatLog() : Id;
    }
}

public record RunContext(
    DateTimeOffset CreatedAt,
    DiscordUser User,
    IUser? FetchedUser,
    CommandChannel Channel,
    CommandGuild? Guild,
    IDiscordClient Client,
    ISelfUser BotUser,
    RunContext.CurrentCommandInfo CommandInfo,
    Lazy<Task<string>> CommandPrefix,
    RunContext.OnGoingState OnGoing,
    CommandActivity Activity,
    bool WasAcknowledged = true
)
{
    public record CurrentCommandInfo(string Id, string Name);
    public class OnGoingState { public string? OnGoingCommandAddedToPool { get; set; } }

    public string MentionCommand(string name) =>
        name.Split(' ')[0] == CommandInfo.Name.Split(' ')[0] ?
            $"</{name}:{CommandInfo.Id}>" :
            $"**/{name}**";

    public GuildTextChannel? GuildTextChannel { get; set; } = Guild != null ? new GuildTextChannel(Channel.Id, Guild.Id, Channel.Type) : null;
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

        foreach (var precondition in _preconditions.Concat(command.Preconditions ?? []))
        {
            if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                return failed;
        }

        var result = await command.RunAsync();

        return result;
    }
}
