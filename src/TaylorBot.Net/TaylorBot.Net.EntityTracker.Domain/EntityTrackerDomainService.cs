using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.EntityTracker.Domain.Guild;
using TaylorBot.Net.EntityTracker.Domain.GuildName;
using TaylorBot.Net.EntityTracker.Domain.Member;
using TaylorBot.Net.EntityTracker.Domain.Options;
using TaylorBot.Net.EntityTracker.Domain.TextChannel;
using TaylorBot.Net.EntityTracker.Domain.User;
using TaylorBot.Net.EntityTracker.Domain.Username;

namespace TaylorBot.Net.EntityTracker.Domain
{
    public class EntityTrackerDomainService
    {
        private readonly ILogger<EntityTrackerDomainService> logger;
        private readonly IOptionsMonitor<EntityTrackerOptions> optionsMonitor;
        private readonly IUserRepository userRepository;
        private readonly IUsernameRepository usernameRepository;
        private readonly ITextChannelRepository textChannelRepository;
        private readonly IGuildRepository guildRepository;
        private readonly IGuildNameRepository guildNameRepository;
        private readonly IMemberRepository memberRepository;

        public EntityTrackerDomainService(
            ILogger<EntityTrackerDomainService> logger,
            IOptionsMonitor<EntityTrackerOptions> optionsMonitor,
            IUserRepository userRepository,
            IUsernameRepository usernameRepository,
            ITextChannelRepository textChannelRepository,
            IGuildRepository guildRepository,
            IGuildNameRepository guildNameRepository,
            IMemberRepository memberRepository)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.userRepository = userRepository;
            this.usernameRepository = usernameRepository;
            this.textChannelRepository = textChannelRepository;
            this.guildRepository = guildRepository;
            this.guildNameRepository = guildNameRepository;
            this.memberRepository = memberRepository;
        }

        public async Task OnGuildJoinedAsync(SocketGuild guild)
        {
            var guildAddedResult = await guildRepository.AddGuildIfNotAddedAsync(guild);

            async Task AddGuildName()
            {
                if (guildAddedResult.WasAdded)
                {
                    logger.LogInformation(LogString.From($"Added new guild {guild.FormatLog()}."));
                    await guildNameRepository.AddNewGuildNameAsync(guild);
                }
                else
                {
                    var latestGuildName = await guildNameRepository.GetLatestGuildNameAsync(guild);
                    if (latestGuildName == default || latestGuildName != guild.Name)
                    {
                        await UpdateGuildNameAsync(guild, latestGuildName);
                    }
                }
            }

            await Task.WhenAll(
                guild.TextChannels.Select(textChannel =>
                    textChannelRepository.AddTextChannelIfNotAddedAsync(textChannel)
                ).Append(
                    AddGuildName()
                ).ToArray()
            );

            await guild.DownloadUsersAsync();

            var allMembers = guild.Users;
            await Task.WhenAll(
                allMembers.Select(async member =>
                {
                    var userAddedResult = await userRepository.AddNewUserAsync(member);
                    if (userAddedResult.WasAdded)
                    {
                        logger.LogInformation(LogString.From($"Added new user from member {member.FormatLog()}."));
                        await memberRepository.AddNewMemberAsync(member);
                        await usernameRepository.AddNewUsernameAsync(member);
                    }
                    else
                    {
                        await memberRepository.AddNewMemberIfNotAddedAsync(member);

                        var latestUsername = await usernameRepository.GetLatestUsernameAsync(member);
                        if (latestUsername == default || latestUsername != member.Username)
                        {
                            await UpdateUsernameAsync(member, latestUsername);
                        }
                    }
                }).Append(
                    memberRepository.SetMembersDeadAsync(guild, allMembers)
                ).ToArray()
            );
        }

        public async Task OnShardReadyAsync(DiscordSocketClient shardClient)
        {
            foreach (var guild in shardClient.Guilds)
            {
                await OnGuildJoinedAsync(guild);
                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenGuildProcessedInReady);
            }
        }

        public async Task OnUserUpdatedAsync(SocketUser oldUser, SocketUser newUser)
        {
            if (oldUser.Username != newUser.Username)
            {
                await UpdateUsernameAsync(newUser, oldUser.Username);
            }
        }

        private async Task UpdateUsernameAsync(IUser user, string previousUsername)
        {
            await usernameRepository.AddNewUsernameAsync(user);
            logger.LogInformation(LogString.From(
                $"Added new username for {user.FormatLog()}{(previousUsername != default ? $", previously was '{previousUsername}'" : "")}."
            ));
        }

        public async Task OnGuildUpdatedAsync(SocketGuild oldGuild, SocketGuild newGuild)
        {
            if (oldGuild.Name != newGuild.Name)
            {
                await UpdateGuildNameAsync(newGuild, oldGuild.Name);
            }
        }

        private async Task UpdateGuildNameAsync(IGuild guild, string previousGuildName)
        {
            await guildNameRepository.AddNewGuildNameAsync(guild);
            logger.LogInformation(LogString.From(
                $"Added new guild name for {guild.FormatLog()}{(previousGuildName != default ? $", previously was '{previousGuildName}'" : "")}."
            ));
        }
    }
}
