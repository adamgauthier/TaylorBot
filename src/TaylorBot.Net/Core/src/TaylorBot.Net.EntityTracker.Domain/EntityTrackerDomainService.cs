using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Events;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.Core.User;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.EntityTracker.Domain;

public partial class EntityTrackerDomainService(
    ILogger<EntityTrackerDomainService> logger,
    IOptionsMonitor<EntityTrackerOptions> optionsMonitor,
    UsernameTrackerDomainService usernameTrackerDomainService,
    IUserRepository userRepository,
    ISpamChannelRepository spamChannelRepository,
    IMemberRepository memberRepository,
    GuildTrackerDomainService guildTrackerDomainService)
{
    private readonly AsyncEvent<Func<IGuildUser, Task>> guildMemberFirstJoinedEvent = new();
    public event Func<IGuildUser, Task> GuildMemberFirstJoinedEvent
    {
        add { guildMemberFirstJoinedEvent.Add(value); }
        remove { guildMemberFirstJoinedEvent.Remove(value); }
    }

    private readonly AsyncEvent<Func<IGuildUser, DateTimeOffset, Task>> guildMemberRejoinedEvent = new();
    public event Func<IGuildUser, DateTimeOffset, Task> GuildMemberRejoinedEvent
    {
        add { guildMemberRejoinedEvent.Add(value); }
        remove { guildMemberRejoinedEvent.Remove(value); }
    }

    public async Task OnGuildJoinedAsync(SocketGuild guild, bool downloadAllUsers)
    {
        await guildTrackerDomainService.TrackGuildAndNameAsync(guild);

        foreach (var textChannel in guild.TextChannels)
        {
            await spamChannelRepository.InsertOrGetIsSpamChannelAsync(new(
                textChannel.Id, textChannel.Guild.Id, textChannel.GetChannelType() ?? ChannelType.Text));
        }

        if (downloadAllUsers)
            await guild.DownloadUsersAsync();

        foreach (var member in guild.Users)
        {
            await OnGuildUserJoinedAsync(member);
        }
    }

    public async Task OnShardReadyAsync(DiscordSocketClient shardClient)
    {
        LogStartingStartupSequence(shardClient.ShardId);

        foreach (var guild in shardClient.Guilds.Where(g => ((IGuild)g).Available))
        {
            await OnGuildJoinedAsync(guild, downloadAllUsers: false);
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenGuildProcessedInReady);
        }

        LogCompletedStartupSequence(shardClient.ShardId);
    }

    public async Task OnUserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
    {
        if (oldUser.Username != newUser.Username)
        {
            DiscordUser user = new(newUser);

            var userAddedResult = await userRepository.AddNewUserAsync(user);

            await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(user, userAddedResult);
        }
    }

    public async Task OnGuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild)
    {
        if (oldGuild.Name != newGuild.Name)
        {
            await guildTrackerDomainService.TrackGuildAndNameAsync(newGuild);
        }
    }

    public async Task OnGuildUserJoinedAsync(SocketGuildUser guildUser)
    {
        DiscordUser user = new(guildUser);

        var userAddedResult = await userRepository.AddNewUserAsync(user);
        await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(user, userAddedResult);

        if (userAddedResult.WasAdded)
        {
            var memberAdded = await memberRepository.AddNewMemberAsync(guildUser);
            if (memberAdded)
            {
                LogAddedNewMember(guildUser.FormatLog());
                await guildMemberFirstJoinedEvent.InvokeAsync(guildUser);
            }
        }
        else
        {
            var memberAddedResult = await memberRepository.AddNewMemberOrUpdateAsync(guildUser);

            if (memberAddedResult is RejoinedMemberAddResult rejoinedMemberAddResult)
            {
                await guildMemberRejoinedEvent.InvokeAsync(guildUser, rejoinedMemberAddResult.FirstJoinedAt);
            }
            else
            {
                await guildMemberFirstJoinedEvent.InvokeAsync(guildUser);
            }
        }
    }

    public async Task OnGuildUserLeftAsync(IGuild guild, IUser user)
    {
        await memberRepository.UpdateMembersNotInGuildAsync(guild, [new SnowflakeId(user.Id)]);
    }

    public async Task OnTextChannelCreatedAsync(SocketTextChannel textChannel)
    {
        _ = await spamChannelRepository.InsertOrGetIsSpamChannelAsync(new(textChannel.Id, textChannel.Guild.Id, textChannel.GetChannelType() ?? ChannelType.Text));
        LogAddedNewTextChannel(textChannel.FormatLog());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting startup entity tracking sequence for shard {ShardId}.")]
    private partial void LogStartingStartupSequence(int shardId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Completed startup entity tracking sequence for shard {ShardId}.")]
    private partial void LogCompletedStartupSequence(int shardId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new member {GuildUser}.")]
    private partial void LogAddedNewMember(string guildUser);

    [LoggerMessage(Level = LogLevel.Information, Message = "Added new text channel {TextChannel}.")]
    private partial void LogAddedNewTextChannel(string textChannel);
}
