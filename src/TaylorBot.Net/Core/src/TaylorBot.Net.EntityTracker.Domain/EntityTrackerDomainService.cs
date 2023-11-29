using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Events;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.EntityTracker.Domain;

public class EntityTrackerDomainService
{
    private readonly ILogger<EntityTrackerDomainService> _logger;
    private readonly IOptionsMonitor<EntityTrackerOptions> _optionsMonitor;
    private readonly UsernameTrackerDomainService _usernameTrackerDomainService;
    private readonly IUserRepository _userRepository;
    private readonly ISpamChannelRepository _spamChannelRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly GuildTrackerDomainService _guildTrackerDomainService;

    private readonly AsyncEvent<Func<IGuildUser, Task>> guildMemberFirstJoinedEvent = new AsyncEvent<Func<IGuildUser, Task>>();
    public event Func<IGuildUser, Task> GuildMemberFirstJoinedEvent
    {
        add { guildMemberFirstJoinedEvent.Add(value); }
        remove { guildMemberFirstJoinedEvent.Remove(value); }
    }

    private readonly AsyncEvent<Func<IGuildUser, DateTimeOffset, Task>> guildMemberRejoinedEvent = new AsyncEvent<Func<IGuildUser, DateTimeOffset, Task>>();
    public event Func<IGuildUser, DateTimeOffset, Task> GuildMemberRejoinedEvent
    {
        add { guildMemberRejoinedEvent.Add(value); }
        remove { guildMemberRejoinedEvent.Remove(value); }
    }

    public EntityTrackerDomainService(
        ILogger<EntityTrackerDomainService> logger,
        IOptionsMonitor<EntityTrackerOptions> optionsMonitor,
        UsernameTrackerDomainService usernameTrackerDomainService,
        IUserRepository userRepository,
        ISpamChannelRepository spamChannelRepository,
        IMemberRepository memberRepository,
        GuildTrackerDomainService guildTrackerDomainService)
    {
        _logger = logger;
        _optionsMonitor = optionsMonitor;
        _usernameTrackerDomainService = usernameTrackerDomainService;
        _userRepository = userRepository;
        _spamChannelRepository = spamChannelRepository;
        _memberRepository = memberRepository;
        _guildTrackerDomainService = guildTrackerDomainService;
    }

    public async Task OnGuildJoinedAsync(SocketGuild guild, bool downloadAllUsers)
    {
        await _guildTrackerDomainService.TrackGuildAndNameAsync(guild);

        foreach (var textChannel in guild.TextChannels)
        {
            await _spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);
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
        _logger.LogInformation($"Starting startup entity tracking sequence for shard {shardClient.ShardId}.");

        foreach (var guild in shardClient.Guilds.Where(g => ((IGuild)g).Available))
        {
            await OnGuildJoinedAsync(guild, downloadAllUsers: false);
            await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenGuildProcessedInReady);
        }

        _logger.LogInformation($"Completed startup entity tracking sequence for shard {shardClient.ShardId}.");
    }

    public async Task OnUserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
    {
        if (oldUser.Username != newUser.Username)
        {
            var userAddedResult = await _userRepository.AddNewUserAsync(newUser);

            await _usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(newUser, userAddedResult);
        }
    }

    public async Task OnGuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild)
    {
        if (oldGuild.Name != newGuild.Name)
        {
            await _guildTrackerDomainService.TrackGuildAndNameAsync(newGuild);
        }
    }

    public async Task OnGuildUserJoinedAsync(SocketGuildUser guildUser)
    {
        var userAddedResult = await _userRepository.AddNewUserAsync(guildUser);
        await _usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(guildUser, userAddedResult);

        if (userAddedResult.WasAdded)
        {
            var memberAdded = await _memberRepository.AddNewMemberAsync(guildUser);
            if (memberAdded)
            {
                _logger.LogInformation($"Added new member {guildUser.FormatLog()}.");
                await guildMemberFirstJoinedEvent.InvokeAsync(guildUser);
            }
        }
        else
        {
            var memberAddedResult = await _memberRepository.AddNewMemberOrUpdateAsync(guildUser);

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
        await _memberRepository.UpdateMembersNotInGuildAsync(guild, new[] { new SnowflakeId(user.Id) });
    }

    public async Task OnTextChannelCreatedAsync(SocketTextChannel textChannel)
    {
        await _spamChannelRepository.InsertOrGetIsSpamChannelAsync(textChannel);
        _logger.LogInformation($"Added new text channel {textChannel.FormatLog()}.");
    }
}
