using Discord;
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

public partial class BirthdayRewardNotifierDomainService(
    ILogger<BirthdayRewardNotifierDomainService> logger,
    IOptionsMonitor<BirthdayRewardNotifierOptions> optionsMonitor,
    IBirthdayRepository birthdayRepository,
    BirthdayRewardEmbedFactory birthdayRewardEmbedFactory,
    Lazy<ITaylorBotClient> taylorBotClient)
{
    public async Task StartCheckingBirthdaysAsync()
    {
        while (true)
        {
            try
            {
                await RewardBirthdaysAsync();
            }
            catch (Exception e)
            {
                LogUnhandledExceptionRewardingBirthdays(e);
                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
                continue;
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRewards);
        }
    }

    public async ValueTask RewardBirthdaysAsync()
    {
        var rewardAmount = optionsMonitor.CurrentValue.RewardAmount;
        LogRewardingEligibleUsers(rewardAmount);

        foreach (var rewardedUser in await birthdayRepository.RewardEligibleUsersAsync(rewardAmount))
        {
            try
            {
                LogRewardedBirthdayPoints(rewardAmount, rewardedUser);
                var user = await taylorBotClient.Value.ResolveRequiredUserAsync(rewardedUser.UserId);
                await user.SendMessageAsync(embed: birthdayRewardEmbedFactory.Create(rewardAmount, rewardedUser));
            }
            catch (HttpException e) when (DiscordDmError.IsUndeliverable(e))
            {
                LogCannotNotify(rewardedUser, e.DiscordCode);
            }
            catch (Exception exception)
            {
                LogExceptionNotifyingBirthday(exception, rewardedUser);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Rewarding eligible users with {RewardAmount} birthday point(s).")]
    private partial void LogRewardingEligibleUsers(long rewardAmount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Rewarded {RewardAmount} birthday point(s) to {RewardedUser}.")]
    private partial void LogRewardedBirthdayPoints(long rewardAmount, RewardedUser rewardedUser);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Can't notify {RewardedUser} about their birthday because Discord rejected the DM with {DiscordErrorCode}.")]
    private partial void LogCannotNotify(RewardedUser rewardedUser, DiscordErrorCode? discordErrorCode);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when attempting to notify {RewardedUser} about their birthday reward.")]
    private partial void LogExceptionNotifyingBirthday(Exception exception, RewardedUser rewardedUser);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in RewardBirthdaysAsync.")]
    private partial void LogUnhandledExceptionRewardingBirthdays(Exception exception);
}
