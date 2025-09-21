﻿using Discord;
using Discord.Net;
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
                logger.LogError(e, $"Unhandled exception in {nameof(RemindUsersAsync)}");
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenReminderChecks);
        }
    }

    public async ValueTask RemindUsersAsync()
    {
        logger.LogDebug("Checking for due reminders");

        foreach (var reminder in await reminderRepository.GetDueRemindersAsync())
        {
            try
            {
                await RemindUserAsync(reminder);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception occurred when attempting to notify {Reminder}", reminder);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
        }
    }

    private async Task RemindUserAsync(Reminder reminder)
    {
        logger.LogDebug("Reminding {Reminder}", reminder);

        var user = await taylorBotClient.Value.ResolveRequiredUserAsync(reminder.UserId);

        try
        {
            await user.SendMessageAsync(embed: reminderEmbedFactory.Create(reminder));
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            logger.LogWarning("Could not remind {User} with {Reminder} because of their DM settings", user.FormatLog(), reminder);
            await reminderRepository.RemoveReminderAsync(reminder);
            return;
        }

        logger.LogDebug("Reminded {User} with {Reminder}", user.FormatLog(), reminder);

        await reminderRepository.RemoveReminderAsync(reminder);
    }
}
