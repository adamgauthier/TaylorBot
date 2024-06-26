﻿using Discord;
using Discord.Net;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.BirthdayReward.Domain.DiscordEmbed;
using TaylorBot.Net.BirthdayReward.Domain.Options;
using TaylorBot.Net.Core.Client;

namespace TaylorBot.Net.BirthdayReward.Domain;

public interface IBirthdayRepository
{
    ValueTask<List<RewardedUser>> RewardEligibleUsersAsync(long rewardAmount);
}

public class BirthdayRewardNotifierDomainService(
    ILogger<BirthdayRewardNotifierDomainService> logger,
    IOptionsMonitor<BirthdayRewardNotifierOptions> optionsMonitor,
    IBirthdayRepository birthdayRepository,
    BirthdayRewardEmbedFactory birthdayRewardEmbedFactory,
    Lazy<ITaylorBotClient> taylorBotClient)
{
    public async Task StartCheckingBirthdaysAsync()
    {
        await Task.Delay(TimeSpan.FromMinutes(5));

        while (true)
        {
            try
            {
                await RewardBirthdaysAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(RewardBirthdaysAsync)}.");
                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
                continue;
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRewards);
        }
    }

    public async ValueTask RewardBirthdaysAsync()
    {
        var rewardAmount = optionsMonitor.CurrentValue.RewardAmount;
        logger.LogDebug("Rewarding eligible users with {PointsCountText}.", "birthday point".ToQuantity(rewardAmount));

        foreach (var rewardedUser in await birthdayRepository.RewardEligibleUsersAsync(rewardAmount))
        {
            try
            {
                logger.LogDebug("Rewarded {PointsCountText} to {RewardedUser}.", "birthday point".ToQuantity(rewardAmount), rewardedUser);
                var user = await taylorBotClient.Value.ResolveRequiredUserAsync(rewardedUser.UserId);
                await user.SendMessageAsync(embed: birthdayRewardEmbedFactory.Create(rewardAmount, rewardedUser));
            }
            catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
            {
                logger.LogWarning("Can't notify {RewardedUser} about their birthday because of their DM settings.", rewardedUser);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when attempting to notify {RewardedUser} about their birthday reward.", rewardedUser);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
        }
    }
}
