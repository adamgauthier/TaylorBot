using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;

namespace TaylorBot.Net.Reminder.Domain
{
    public class ReminderNotifierDomainService
    {
        private readonly ILogger<ReminderNotifierDomainService> _logger;
        private readonly IOptionsMonitor<ReminderNotifierOptions> _optionsMonitor;
        private readonly IReminderRepository _reminderRepository;
        private readonly ReminderEmbedFactory _reminderEmbedFactory;
        private readonly Lazy<ITaylorBotClient> _taylorBotClient;

        public ReminderNotifierDomainService(
            ILogger<ReminderNotifierDomainService> logger,
            IOptionsMonitor<ReminderNotifierOptions> optionsMonitor,
            IReminderRepository reminderRepository,
            ReminderEmbedFactory reminderEmbedFactory,
            Lazy<ITaylorBotClient> taylorBotClient
        )
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _reminderRepository = reminderRepository;
            _reminderEmbedFactory = reminderEmbedFactory;
            _taylorBotClient = taylorBotClient;
        }

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
                    _logger.LogError(e, $"Unhandled exception in {nameof(RemindUsersAsync)}.");
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenReminderChecks);
            }
        }

        public async ValueTask RemindUsersAsync()
        {
            foreach (var reminder in await _reminderRepository.GetExpiredRemindersAsync())
            {
                try
                {
                    _logger.LogDebug($"Reminding {reminder}.");
                    var user = await _taylorBotClient.Value.ResolveRequiredUserAsync(reminder.UserId);
                    try
                    {
                        await user.SendMessageAsync(embed: _reminderEmbedFactory.Create(reminder));
                        _logger.LogDebug($"Reminded {user.FormatLog()} with {reminder}.");
                        await _reminderRepository.RemoveReminderAsync(reminder);
                    }
                    catch (Discord.Net.HttpException httpException)
                    {
                        if (httpException.DiscordCode == 50007)
                        {
                            _logger.LogWarning($"Could not remind {user.FormatLog()} with {reminder} because they can't receive DMs.");
                            await _reminderRepository.RemoveReminderAsync(reminder);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Exception occurred when attempting to notify {reminder}.");
                }

                await Task.Delay(_optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
            }
        }
    }
}
