using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.PatreonSync.Domain.DiscordEmbed;
using TaylorBot.Net.PatreonSync.Domain.Options;

namespace TaylorBot.Net.PatreonSync.Domain
{
    public enum PatreonChargeStatus { Paid, Other }

    public record PatronLastCharge(string Date, PatreonChargeStatus Status);

    public record Patron(
        SnowflakeId DiscordUserId,
        bool IsActive,
        PatronLastCharge? LastCharge,
        long CurrentlyEntitledAmountCents,
        string Metadata
    );

    public interface IPatreonClient
    {
        ValueTask<IReadOnlyCollection<Patron>> GetPatronsWithDiscordAccountAsync(uint campaignId);
    }

    public interface IUpdatePlusUserResult { }
    public record UserRewarded(long Reward, long NewTaypointCount) : IUpdatePlusUserResult;
    public record GuildsDisabledForLoweredPledge(IReadOnlyCollection<string> DisabledGuilds, long MaxPlusGuilds) : IUpdatePlusUserResult;
    public record GuildsDisabledForInactivity(IReadOnlyCollection<string> DisabledGuilds) : IUpdatePlusUserResult;
    public record UserUpdated() : IUpdatePlusUserResult;
    public record ActiveUserAdded() : IUpdatePlusUserResult;
    public record InactiveUserAdded() : IUpdatePlusUserResult;

    public interface IPlusRepository
    {
        ValueTask<IUpdatePlusUserResult> AddOrUpdatePlusUserAsync(Patron patron);
    }

    public class PatreonSyncDomainService
    {
        private readonly ILogger<PatreonSyncDomainService> _logger;
        private readonly IOptionsMonitor<PatreonSyncOptions> _optionsMonitor;
        private readonly IPatreonClient _patreonClient;
        private readonly IPlusRepository _plusRepository;
        private readonly Lazy<ITaylorBotClient> _taylorBotClient;

        public PatreonSyncDomainService(
            ILogger<PatreonSyncDomainService> logger,
            IOptionsMonitor<PatreonSyncOptions> optionsMonitor,
            IPatreonClient patreonClient,
            IPlusRepository plusRepository,
            Lazy<ITaylorBotClient> taylorBotClient
        )
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _patreonClient = patreonClient;
            _plusRepository = plusRepository;
            _taylorBotClient = taylorBotClient;
        }

        public async Task StartSyncingPatreonSupportersAsync()
        {
            while (true)
            {
                try
                {
                    if (_optionsMonitor.CurrentValue.Enabled)
                        await SyncPatreonSupportersAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unhandled exception in {nameof(SyncPatreonSupportersAsync)}.");
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenSyncs);
            }
        }

        private async ValueTask SyncPatreonSupportersAsync()
        {
            _logger.LogInformation("Syncing Patreon supporters.");

            foreach (var patron in await _patreonClient.GetPatronsWithDiscordAccountAsync(_optionsMonitor.CurrentValue.CampaignId))
            {
                try
                {
                    var result = await _plusRepository.AddOrUpdatePlusUserAsync(patron);

                    var embed = PatreonUpdateEmbedFactory.Create(result);

                    if (embed != null)
                    {
                        var user = await _taylorBotClient.Value.ResolveRequiredUserAsync(patron.DiscordUserId);
                        await user.SendMessageAsync(embed: embed);
                        await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Exception occurred when attempting to sync patron {patron}.");
                }
            }
        }
    }
}
