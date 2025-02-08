using Discord;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.Parsers.Channels;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;

namespace TaylorBot.Net.Commands;

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
    DiscordChannel Channel,
    CommandGuild? Guild,
    IDiscordClient Client,
    RunContext.CurrentCommandInfo CommandInfo,
    Lazy<Task<string>> CommandPrefix,
    RunContext.OnGoingState OnGoing,
    CommandActivity Activity,
    bool WasAcknowledged = true
)
{
    public record CurrentCommandInfo(string Id, string Name);
    public class OnGoingState { public string? OnGoingCommandAddedToPool { get; set; } }

    public string MentionCommand(Command command) => !string.IsNullOrEmpty(CommandInfo.Name)
        ? MentionCommand(command.Metadata.Name)
        : $"**{command.Metadata.Name}**";

    public string MentionCommand(string name) =>
        name.Split(' ')[0] == CommandInfo.Name.Split(' ')[0] ?
            $"</{name}:{CommandInfo.Id}>" :
            $"**/{name}**";

    public GuildTextChannel? GuildTextChannel { get; set; } = Guild != null ? new GuildTextChannel(Channel.Id, Guild.Id, Channel.Type) : null;
}

public class RunContextFactory(
    Lazy<ITaylorBotClient> taylorBotClient,
    InteractionMapper interactionMapper,
    CommandPrefixDomainService commandPrefixDomainService)
{
    public RunContext BuildContext(ParsedInteraction interaction, CommandActivity activity, bool wasAcknowledged)
    {
        var guild = interaction.Guild != null
            ? taylorBotClient.Value.DiscordShardedClient.GetGuild(interaction.Guild.Id)
            : null;

        var user = interaction.User;

        return new RunContext(
            CreatedAt: DateTimeOffset.UtcNow,
            User: new(
                user.id,
                user.username,
                user.avatar,
                user.discriminator,
                IsBot: user.bot == true,
                interaction.Guild != null
                    ? interactionMapper.ToMemberInfo(interaction.Guild.Id, interaction.Guild.Member)
                    : null),
            FetchedUser: null,
            Channel: new(interaction.Channel.Id, (ChannelType)interaction.Channel.Partial.type),
            Guild: interaction.Guild != null ? new(interaction.Guild.Id, guild) : null,
            Client: taylorBotClient.Value.DiscordShardedClient,
            CommandInfo: new(interaction.Data.id, interaction.Data.name),
            CommandPrefix: new(() => commandPrefixDomainService.GetPrefixAsync(guild)),
            OnGoing: new(),
            activity,
            WasAcknowledged: wasAcknowledged
        );
    }
}

public record Command(CommandMetadata Metadata, Func<ValueTask<ICommandResult>> RunAsync, IList<ICommandPrecondition>? Preconditions = null);

public record CommandMetadata(string Name, string? ModuleName = null, IReadOnlyList<string>? Aliases = null);

public interface ICommandRunner
{
    ValueTask<ICommandResult> RunSlashCommandAsync(Command command, RunContext context);
    Task<ICommandResult> RunInteractionAsync(Command command, RunContext context);
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
    private readonly List<ICommandPrecondition> slashCommandsPreconditions = [notDisabled, notGuildDisabled, notGuildChannelDisabled, userNotIgnored, memberTracked, textChannelTracked, userNoOngoingCommand];

    public async ValueTask<ICommandResult> RunSlashCommandAsync(Command command, RunContext context)
    {
        context.Activity.CommandName = command.Metadata.Name;
        context.Activity.UserId = context.User.Id;
        context.Activity.ChannelId = context.Channel.Id;
        context.Activity.GuildId = context.Guild?.Id;

        foreach (var precondition in slashCommandsPreconditions.Concat(command.Preconditions ?? []))
        {
            if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                return failed;
        }

        var result = await command.RunAsync();

        return result;
    }

    private readonly List<ICommandPrecondition> interactionsPreconditions = [userNotIgnored, memberTracked, textChannelTracked];

    public async Task<ICommandResult> RunInteractionAsync(Command command, RunContext context)
    {
        context.Activity.CommandName = command.Metadata.Name;
        context.Activity.UserId = context.User.Id;
        context.Activity.ChannelId = context.Channel.Id;
        context.Activity.GuildId = context.Guild?.Id;

        foreach (var precondition in interactionsPreconditions.Concat(command.Preconditions ?? []))
        {
            if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                return failed;
        }

        var result = await command.RunAsync();

        return result;
    }
}
