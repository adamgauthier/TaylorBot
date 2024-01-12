using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Snowflake;
using TaylorBot.Net.PatreonSync.Domain.DiscordEmbed;
using TaylorBot.Net.PatreonSync.Domain.Options;

namespace TaylorBot.Net.PatreonSync.Domain;

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

public class PatreonSyncDomainService(
    ILogger<PatreonSyncDomainService> logger,
    IOptionsMonitor<PatreonSyncOptions> optionsMonitor,
    IPatreonClient patreonClient,
    IPlusRepository plusRepository,
    Lazy<ITaylorBotClient> taylorBotClient
    )
{
    public async Task StartSyncingPatreonSupportersAsync()
    {
        while (true)
        {
            try
            {
                if (optionsMonitor.CurrentValue.Enabled)
                    await SyncPatreonSupportersAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(SyncPatreonSupportersAsync)}.");
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenSyncs);
        }
    }

    private async ValueTask SyncPatreonSupportersAsync()
    {
        logger.LogInformation("Syncing Patreon supporters.");

        foreach (var patron in await patreonClient.GetPatronsWithDiscordAccountAsync(optionsMonitor.CurrentValue.CampaignId))
        {
            try
            {
                var result = await plusRepository.AddOrUpdatePlusUserAsync(patron);

                var embed = PatreonUpdateEmbedFactory.Create(result);

                if (embed != null)
                {
                    var user = await taylorBotClient.Value.ResolveRequiredUserAsync(patron.DiscordUserId);
                    await user.SendMessageAsync(embed: embed);
                    await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when attempting to sync patron {Patron}.", patron);
            }
        }
    }
}
