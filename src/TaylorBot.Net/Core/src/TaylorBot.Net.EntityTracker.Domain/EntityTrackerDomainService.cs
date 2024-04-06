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

public class EntityTrackerDomainService(
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
            await spamChannelRepository.InsertOrGetIsSpamChannelAsync(new(textChannel.Id, textChannel.Guild.Id));
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
        logger.LogInformation("Starting startup entity tracking sequence for shard {ShardId}.", shardClient.ShardId);

        foreach (var guild in shardClient.Guilds.Where(g => ((IGuild)g).Available))
        {
            await OnGuildJoinedAsync(guild, downloadAllUsers: false);
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenGuildProcessedInReady);
        }

        logger.LogInformation("Completed startup entity tracking sequence for shard {ShardId}.", shardClient.ShardId);
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
                logger.LogInformation("Added new member {GuildUser}.", guildUser.FormatLog());
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
        await spamChannelRepository.InsertOrGetIsSpamChannelAsync(new(textChannel.Id, textChannel.Guild.Id));
        logger.LogInformation("Added new text channel {TextChannel}.", textChannel.FormatLog());
    }
}
