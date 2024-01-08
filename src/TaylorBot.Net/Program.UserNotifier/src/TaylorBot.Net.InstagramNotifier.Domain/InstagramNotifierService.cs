using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.InstagramNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.InstagramNotifier.Domain.Options;

namespace TaylorBot.Net.InstagramNotifier.Domain;

public class InstagramNotifierService(
    ILogger<InstagramNotifierService> logger,
    IOptionsMonitor<InstagramNotifierOptions> optionsMonitor,
    IInstagramCheckerRepository instagramCheckerRepository,
    IInstagramClient instagramClient,
    InstagramPostToEmbedMapper instagramPostToEmbedMapper,
    Lazy<ITaylorBotClient> taylorBotClient
    )
{
    public async Task StartCheckingInstagramsAsync()
    {
        while (true)
        {
            try
            {
                await CheckAllInstagramsAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(CheckAllInstagramsAsync)}.");
            }
            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }

    public async ValueTask CheckAllInstagramsAsync()
    {
        foreach (var instagramChecker in await instagramCheckerRepository.GetInstagramCheckersAsync())
        {
            try
            {
                var channel = taylorBotClient.Value.ResolveRequiredGuild(instagramChecker.GuildId).GetRequiredTextChannel(instagramChecker.ChannelId);

                var newestPost = await instagramClient.GetLatestPostAsync(instagramChecker.InstagramUsername);

                if (newestPost.ShortCode != instagramChecker.LastPostCode && newestPost.TakenAt > instagramChecker.LastPostTakenAt)
                {
                    logger.LogDebug($"Found new Instagram post for {instagramChecker}: {newestPost.ShortCode}.");
                    await channel.SendMessageAsync(embed: instagramPostToEmbedMapper.ToEmbed(newestPost));
                    await instagramCheckerRepository.UpdateLastPostAsync(instagramChecker, newestPost);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Exception occurred when checking {instagramChecker}.");
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
        }
    }
}
