using Discord;
using TaylorBot.Net.Core.Colors;

namespace TaylorBot.Net.Reminder.Domain.DiscordEmbed
{
    public class ReminderEmbedFactory
    {
        public Embed Create(Reminder reminder)
        {
            return new EmbedBuilder()
                .WithColor(TaylorBotColors.SuccessColor)
                .WithDescription(reminder.ReminderText)
                .WithTitle("Reminder")
                .WithTimestamp(reminder.CreatedAt)
                .Build();
        }
    }
}
