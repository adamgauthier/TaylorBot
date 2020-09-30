using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Events;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Domain.GuildName;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.User;

namespace TaylorBot.Net.EntityTracker.Domain
{
    public class EntityTrackerDomainService
    {
        private readonly ILogger<EntityTrackerDomainService> logger;
        private readonly IOptionsMonitor<EntityTrackerOptions> optionsMonitor;
        private readonly UsernameTrackerDomainService usernameTrackerDomainService;
        private readonly IUserRepository userRepository;
        private readonly ITextChannelRepository textChannelRepository;
        private readonly IGuildRepository guildRepository;
        private readonly IGuildNameRepository guildNameRepository;
        private readonly IMemberRepository memberRepository;

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
            ITextChannelRepository textChannelRepository,
            IGuildRepository guildRepository,
            IGuildNameRepository guildNameRepository,
            IMemberRepository memberRepository)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.usernameTrackerDomainService = usernameTrackerDomainService;
            this.userRepository = userRepository;
            this.textChannelRepository = textChannelRepository;
            this.guildRepository = guildRepository;
            this.guildNameRepository = guildNameRepository;
            this.memberRepository = memberRepository;
        }

        public async Task OnGuildJoinedAsync(SocketGuild guild, bool downloadAllUsers)
        {
            var guildAddedResult = await guildRepository.AddGuildIfNotAddedAsync(guild);

            async Task AddGuildNameAsync()
            {
                if (guildAddedResult.WasAdded)
                {
                    logger.LogInformation(LogString.From($"Added new guild {guild.FormatLog()}."));
                    await guildNameRepository.AddNewGuildNameAsync(guild);
                }
                else if (guildAddedResult.WasGuildNameChanged)
                {
                    await UpdateGuildNameAsync(guild, guildAddedResult.PreviousGuildName);
                }
            }

            await Task.WhenAll(
                guild.TextChannels.Select(textChannel =>
                    textChannelRepository.AddTextChannelIfNotAddedAsync(textChannel)
                ).Append(
                    AddGuildNameAsync()
                ).ToArray()
            );

            if (downloadAllUsers)
                await guild.DownloadUsersAsync();

            foreach (var member in guild.Users)
            {
                await OnGuildUserJoinedAsync(member);
            }
        }

        public async Task OnShardReadyAsync(DiscordSocketClient shardClient)
        {
            foreach (var guild in shardClient.Guilds.Where(g => ((IGuild)g).Available))
            {
                await OnGuildJoinedAsync(guild, downloadAllUsers: false);
                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenGuildProcessedInReady);
            }
        }

        public async Task OnUserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
        {
            if (oldUser.Username != newUser.Username)
            {
                var userAddedResult = await userRepository.AddNewUserAsync(newUser);

                await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(newUser, userAddedResult);
            }
        }

        public async Task OnGuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild)
        {
            if (oldGuild.Name != newGuild.Name)
            {
                var guildAddedResult = await guildRepository.AddGuildIfNotAddedAsync(newGuild);

                if (guildAddedResult.WasAdded)
                {
                    logger.LogInformation(LogString.From($"Added new guild {newGuild.FormatLog()}."));
                    await guildNameRepository.AddNewGuildNameAsync(newGuild);
                }
                else if (guildAddedResult.WasGuildNameChanged)
                {
                    await UpdateGuildNameAsync(newGuild, guildAddedResult.PreviousGuildName);
                }
            }
        }

        private async Task UpdateGuildNameAsync(IGuild guild, string? previousGuildName)
        {
            await guildNameRepository.AddNewGuildNameAsync(guild);
            logger.LogInformation(LogString.From(
                $"Added new guild name for {guild.FormatLog()}{(previousGuildName != null ? $", previously was '{previousGuildName}'" : "")}."
            ));
        }

        public async Task OnGuildUserJoinedAsync(SocketGuildUser guildUser)
        {
            var userAddedResult = await userRepository.AddNewUserAsync(guildUser);
            await usernameTrackerDomainService.AddUsernameAfterUserAddedAsync(guildUser, userAddedResult);

            if (userAddedResult.WasAdded)
            {
                var memberAdded = await memberRepository.AddNewMemberAsync(guildUser);
                if (memberAdded)
                {
                    logger.LogInformation(LogString.From($"Added new member {guildUser.FormatLog()}."));
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

        public async Task OnGuildUserLeftAsync(SocketGuildUser guildUser)
        {
            await memberRepository.SetMemberDeadAsync(guildUser);
        }

        public async Task OnTextChannelCreatedAsync(SocketTextChannel textChannel)
        {
            await textChannelRepository.AddTextChannelAsync(textChannel);
            logger.LogInformation(LogString.From($"Added new text channel {textChannel.FormatLog()}."));
        }
    }
}
