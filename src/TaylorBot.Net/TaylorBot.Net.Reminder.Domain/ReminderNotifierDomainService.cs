using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TaylorBot.Net.Reminder.Domain.DiscordEmbed;
using TaylorBot.Net.Reminder.Domain.Options;
using TaylorBot.Net.Core.Client;
using TaylorBot.Net.Core.Logging;

namespace TaylorBot.Net.Reminder.Domain
{
    public class ReminderNotifierDomainService
    {
        private readonly ILogger<ReminderNotifierDomainService> logger;
        private readonly IOptionsMonitor<ReminderNotifierOptions> optionsMonitor;
        private readonly IReminderRepository reminderRepository;
        private readonly ReminderEmbedFactory reminderEmbedFactory;
        private readonly TaylorBotClient taylorBotClient;

        public ReminderNotifierDomainService(
            ILogger<ReminderNotifierDomainService> logger,
            IOptionsMonitor<ReminderNotifierOptions> optionsMonitor,
            IReminderRepository reminderRepository,
            ReminderEmbedFactory reminderEmbedFactory,
            TaylorBotClient taylorBotClient)
        {
            this.logger = logger;
            this.optionsMonitor = optionsMonitor;
            this.reminderEmbedFactory = reminderEmbedFactory;
            this.reminderRepository = reminderRepository;
            this.taylorBotClient = taylorBotClient;
        }

        public async Task StartReminderCheckerAsync()
        {
            while (true)
            {
                foreach (var reminder in await reminderRepository.GetExpiredRemindersAsync())
                {
                    try
                    {
                        logger.LogTrace(LogString.From($"Reminding {reminder}."));
                        var user = await taylorBotClient.ResolveRequiredUserAsync(reminder.UserId);
                        try
                        {
                            await user.SendMessageAsync(embed: reminderEmbedFactory.Create(reminder));
                            logger.LogTrace(LogString.From($"Reminded {user.FormatLog()} with {reminder}."));
                            await reminderRepository.RemoveReminderAsync(reminder);
                        }
                        catch (Discord.Net.HttpException httpException)
                        {
                            if (httpException.DiscordCode == 50007)
                            {
                                logger.LogWarning(LogString.From($"Could not remind {user.FormatLog()} with {reminder} because they can't receive DMs."));
                                await reminderRepository.RemoveReminderAsync(reminder);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, LogString.From($"Exception occurred when attempting to notify {reminder}."));
                    }

                    await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenMessages);
                }

                await Task.Delay(optionsMonitor.CurrentValue.TimeSpanBetweenReminderChecks);
            }
        }
    }
}
