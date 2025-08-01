﻿using Discord;
using TaylorBot.Net.Commands.Instrumentation;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Channel;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using static TaylorBot.Net.Commands.RunContext;

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
    GuildTextChannel? GuildTextChannel,
    IDiscordClient Client,
    SlashCommandInfo? SlashCommand,
    OnGoingState OnGoing,
    CommandActivity Activity,
    bool WasAcknowledged = true,
    PrefixCommandInfo? PrefixCommand = null
)
{
    public record SlashCommandInfo(string Id, string Name);

    public record PrefixCommandInfo(IReadOnlyList<string>? ReplacementSlashCommands, bool IsRemoved = false, string? RemovedMessage = null)
    {
        public PrefixCommandInfo(string? ReplacementSlashCommand = null, bool IsRemoved = false, string? RemovedMessage = null) : this(ReplacementSlashCommand != null ? [ReplacementSlashCommand] : null, IsRemoved, RemovedMessage) { }
    }

    public class OnGoingState { public string? OnGoingCommandAddedToPool { get; set; } }
}

public class RunContextFactory(
    Lazy<ITaylorBotClient> taylorBotClient,
    InteractionMapper interactionMapper,
    TimeProvider timeProvider)
{
    public RunContext BuildContext(ParsedInteraction interaction, CommandActivity activity, bool wasAcknowledged)
    {
        var fetchedGuild = interaction.Guild != null
            ? taylorBotClient.Value.DiscordShardedClient.GetGuild(interaction.Guild.Id)
            : null;

        CommandGuild? guild = interaction.Guild != null
            ? new(interaction.Guild.Id, fetchedGuild)
            : null;

        DiscordChannel channel = new(interaction.Channel.Id, (ChannelType)interaction.Channel.Partial.type);

        var user = interaction.User;

        RunContext.SlashCommandInfo? CreateSlashCommandInfo()
        {
            if (interaction.Raw.type is 2 or 4)
            {
                if (!interaction.Data.id.HasValue)
                {
                    throw new ArgumentNullException(nameof(interaction.Data.id));
                }

                var stringId = interaction.Data.id.Value.GetString();
                ArgumentNullException.ThrowIfNull(stringId);

                ArgumentNullException.ThrowIfNull(interaction.Data.name);
                return new(stringId, interaction.Data.name);
            }

            return null;
        }

        return new RunContext(
            CreatedAt: timeProvider.GetUtcNow(),
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
            Channel: channel,
            Guild: guild,
            GuildTextChannel: guild != null ? new GuildTextChannel(channel.Id, guild.Id, channel.Type) : null,
            Client: taylorBotClient.Value.DiscordShardedClient,
            SlashCommand: CreateSlashCommandInfo(),
            OnGoing: new(),
            activity,
            WasAcknowledged: wasAcknowledged
        );
    }
}

public record Command(CommandMetadata Metadata, Func<ValueTask<ICommandResult>> RunAsync, IList<ICommandPrecondition>? Preconditions = null);

public record CommandMetadata(string Name, IReadOnlyList<string>? Aliases = null, bool IsSlashCommand = true);

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
    UserNoOngoingCommandPrecondition userNoOngoingCommand,
    NotRemovedPrecondition notRemoved
    ) : ICommandRunner
{
    private readonly List<ICommandPrecondition> slashCommandsPreconditions = [notDisabled, notGuildDisabled, notGuildChannelDisabled, userNotIgnored, memberTracked, textChannelTracked, userNoOngoingCommand, notRemoved];

    public async ValueTask<ICommandResult> RunSlashCommandAsync(Command command, RunContext context)
    {
        SetActivityInfo(command, context);

        var preconditions = command.Preconditions != null ? slashCommandsPreconditions.Concat(command.Preconditions) : slashCommandsPreconditions;
        foreach (var precondition in preconditions)
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
        SetActivityInfo(command, context);

        // Some interactions require instant responses (modals), so we skip precondition checks (we'll check on the modal submit)
        var basePreconditions = context.WasAcknowledged ? interactionsPreconditions : [];

        var preconditions = command.Preconditions != null ? basePreconditions.Concat(command.Preconditions) : basePreconditions;
        foreach (var precondition in preconditions)
        {
            if (await precondition.CanRunAsync(command, context) is PreconditionFailed failed)
                return failed;
        }

        var result = await command.RunAsync();

        return result;
    }

    private static void SetActivityInfo(Command command, RunContext context)
    {
        context.Activity.SetCommandName(command.Metadata.Name);
        context.Activity.SetUserId(context.User.Id);
        context.Activity.SetChannelId(context.Channel.Id);
        context.Activity.SetGuildId(context.Guild?.Id);
    }
}
