using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;

namespace TaylorBot.Net.Reminder.Domain;

public class ReminderNotifierDomainService(
    ILogger<ReminderNotifierDomainService> logger,
    IOptionsMonitor<ReminderNotifierOptions> optionsMonitor,
    IReminderRepository reminderRepository,
    ReminderEmbedFactory reminderEmbedFactory,
    Lazy<ITaylorBotClient> taylorBotClient
    )
{
    public async Task StartCheckingRemindersAsync()
    {
        while (true)
        {
            try
            {
                await RemindUsersAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in {nameof(RemindUsersAsync)}.");
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenReminderChecks);
        }
    }

    public async ValueTask RemindUsersAsync()
    {
        foreach (var reminder in await reminderRepository.GetExpiredRemindersAsync())
        {
            try
            {
                logger.LogDebug("Reminding {Reminder}.", reminder);
                var user = await taylorBotClient.Value.ResolveRequiredUserAsync(reminder.UserId);
                try
                {
                    await user.SendMessageAsync(embed: reminderEmbedFactory.Create(reminder));
                    logger.LogDebug("Reminded {User} with {Reminder}.", user.FormatLog(), reminder);
                    await reminderRepository.RemoveReminderAsync(reminder);
                }
                catch (Discord.Net.HttpException httpException)
                {
                    if (httpException.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                    {
                        logger.LogWarning("Could not remind {User} with {Reminder} because they can't receive DMs.", user.FormatLog(), reminder);
                        await reminderRepository.RemoveReminderAsync(reminder);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when attempting to notify {Reminder}.", reminder);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
        }
    }
}
