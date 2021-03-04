using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.InstagramNotifier.Domain.DiscordEmbed;
using TaylorBot.Net.InstagramNotifier.Domain.Options;

namespace TaylorBot.Net.InstagramNotifier.Domain
{
    public class InstagramNotifierService
    {
        private readonly ILogger<InstagramNotifierService> _logger;
        private readonly IOptionsMonitor<InstagramNotifierOptions> _optionsMonitor;
        private readonly IInstagramCheckerRepository _instagramCheckerRepository;
        private readonly IInstagramClient _instagramClient;
        private readonly InstagramPostToEmbedMapper _instagramPostToEmbedMapper;
        private readonly ITaylorBotClient _taylorBotClient;

        public InstagramNotifierService(
            ILogger<InstagramNotifierService> logger,
            IOptionsMonitor<InstagramNotifierOptions> optionsMonitor,
            IInstagramCheckerRepository instagramCheckerRepository,
            IInstagramClient instagramClient,
            InstagramPostToEmbedMapper instagramPostToEmbedMapper,
            ITaylorBotClient taylorBotClient
        )
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _instagramCheckerRepository = instagramCheckerRepository;
            _instagramClient = instagramClient;
            _instagramPostToEmbedMapper = instagramPostToEmbedMapper;
            _taylorBotClient = taylorBotClient;
        }

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
                    _logger.LogError(e, $"Unhandled exception in {nameof(CheckAllInstagramsAsync)}.");
                }
                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }

        public async ValueTask CheckAllInstagramsAsync()
        {
            foreach (var instagramChecker in await _instagramCheckerRepository.GetInstagramCheckersAsync())
            {
                try
                {
                    var channel = _taylorBotClient.ResolveRequiredGuild(instagramChecker.GuildId).GetRequiredTextChannel(instagramChecker.ChannelId);

                    var newestPost = await _instagramClient.GetLatestPostAsync(instagramChecker.InstagramUsername);

                    if (newestPost.ShortCode != instagramChecker.LastPostCode && newestPost.TakenAt > instagramChecker.LastPostTakenAt)
                    {
                        _logger.LogDebug($"Found new Instagram post for {instagramChecker}: {newestPost.ShortCode}.");
                        await channel.SendMessageAsync(embed: _instagramPostToEmbedMapper.ToEmbed(newestPost));
                        await _instagramCheckerRepository.UpdateLastPostAsync(instagramChecker, newestPost);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Exception occurred when checking {instagramChecker}.");
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenRequests);
            }
        }
    }
}
