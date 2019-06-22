using Discord;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.BirthdayReward.Domain
{
    public class BirthdayRewardNotifierDomainService
    {
        private readonly ILogger<BirthdayRewardNotifierDomainService> logger;
        private readonly IOptionsMonitor<BirthdayRewardNotifierOptions> optionsMonitor;
        private readonly IBirthdayRepository birthdayRepository;
        private readonly BirthdayRewardEmbedFactory birthdayRewardEmbedFactory;
        private readonly TaylorBotClient taylorBotClient;

        public BirthdayRewardNotifierDomainService(
            ILogger<BirthdayRewardNotifierDomainService> logger,
            IOptionsMonitor<BirthdayRewardNotifierOptions> optionsMonitor,
            IBirthdayRepository birthdayRepository,
            BirthdayRewardEmbedFactory birthdayRewardEmbedFactory,
            TaylorBotClient taylorBotClient)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.birthdayRewardEmbedFactory = birthdayRewardEmbedFactory;
            this.birthdayRepository = birthdayRepository;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task StartBirthdayRewardCheckerAsync()
        {
            while (true)
            {
                var rewardAmount = optionsMonitor.CurrentValue.RewardAmount;
                logger.LogTrace(LogString.From($"Rewarding eligible users with {"birthday point".ToQuantity(rewardAmount)}."));
                foreach (var rewardedUser in await birthdayRepository.RewardEligibleUsersAsync(rewardAmount))
                {
                    try
                    {
                        logger.LogTrace(LogString.From($"Rewarded {"birthday point".ToQuantity(rewardAmount)} to {rewardedUser}."));
                        var user = await taylorBotClient.ResolveRequiredUserAsync(rewardedUser.UserId);
                        await user.SendMessageAsync(embed: birthdayRewardEmbedFactory.Create(rewardAmount, rewardedUser));
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, LogString.From($"Exception occurred when attempting to notify {rewardedUser} about their birthday reward."));
                    }

                    await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
                }

                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRewards);
            }
        }
    }
}
