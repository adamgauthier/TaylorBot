using Discord;
using Discord.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;

namespace TaylorBot.Net.Reminder.Domain;

public partial class ReminderNotifierDomainService(
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
                LogUnhandledExceptionRemindingUsers(e);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenReminderChecks);
        }
    }

    public async ValueTask RemindUsersAsync()
    {
        LogCheckingForDueReminders();

        foreach (var reminder in await reminderRepository.GetDueRemindersAsync())
        {
            try
            {
                await RemindUserAsync(reminder);
            }
            catch (Exception exception)
            {
                LogExceptionNotifyingReminder(exception, reminder);
            }

            await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
        }
    }

    private async Task RemindUserAsync(Reminder reminder)
    {
        LogRemindingUser(reminder);

        var user = await taylorBotClient.Value.ResolveRequiredUserAsync(reminder.UserId);

        try
        {
            await user.SendMessageAsync(embed: reminderEmbedFactory.Create(reminder));
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            LogCouldNotRemindDueToSettings(user.FormatLog(), reminder);
            await reminderRepository.RemoveReminderAsync(reminder);
            return;
        }

        LogRemindedUser(user.FormatLog(), reminder);

        await reminderRepository.RemoveReminderAsync(reminder);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Checking for due reminders")]
    private partial void LogCheckingForDueReminders();

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred when attempting to notify {Reminder}")]
    private partial void LogExceptionNotifyingReminder(Exception exception, Reminder reminder);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Reminding {Reminder}")]
    private partial void LogRemindingUser(Reminder reminder);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not remind {User} with {Reminder} because of their DM settings")]
    private partial void LogCouldNotRemindDueToSettings(string user, Reminder reminder);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Reminded {User} with {Reminder}")]
    private partial void LogRemindedUser(string user, Reminder reminder);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in " + nameof(RemindUsersAsync) + ".")]
    private partial void LogUnhandledExceptionRemindingUsers(Exception exception);
}
