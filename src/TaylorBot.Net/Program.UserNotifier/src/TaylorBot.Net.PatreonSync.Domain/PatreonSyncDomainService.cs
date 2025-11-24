using Discord;
using Microsoft.Extensions.DependencyInjection;
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

public partial class PatreonSyncDomainService(
    IServiceProvider serviceProvider,
    ILogger<PatreonSyncDomainService> logger,
    IOptionsMonitor<PatreonSyncOptions> optionsMonitor,
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
                LogUnhandledExceptionSyncingPatreon(e);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenSyncs);
        }
    }

    private async ValueTask SyncPatreonSupportersAsync()
    {
        LogSyncingPatreonSupporters();

        var patreonClient = serviceProvider.GetRequiredService<IPatreonClient>();

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
                LogExceptionSyncingPatron(exception, patron);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when attempting to sync patron {Patron}.")]
    private partial void LogExceptionSyncingPatron(Exception exception, Patron patron);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in " + nameof(SyncPatreonSupportersAsync) + ".")]
    private partial void LogUnhandledExceptionSyncingPatreon(Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Syncing Patreon supporters.")]
    private partial void LogSyncingPatreonSupporters();
}
